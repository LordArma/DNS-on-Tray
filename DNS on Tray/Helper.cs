using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace DNS_on_Tray
{
    public static class Helper
    {
        public static bool IsAdministrator
        {
            get
            {
                var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

                return pricipal != null && pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public enum DnsChangeResult
        {
            Success,
            Cancelled,
            NoAdapter,
            InvalidAddress,
            Failed
        }

        public sealed record Adapter(string Id, string Name, int Index, bool IsUp);

        public sealed record CurrentDnsInfo(string AdapterName, bool Automatic, List<string> Servers);

        private const string SettingsKey = "SOFTWARE\\DNS on Tray";
        private const string RunKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string RunValue = "dnsontry";
        public const string ElevatedTaskName = "DNS on Tray";
        public const string FromTaskArgument = "--from-task";

        /// <summary>
        /// Returns true when the text is a plain dotted-quad IPv4 address (e.g. "1.1.1.1").
        /// IPAddress.TryParse alone also accepts forms like "1" or "0x01010101", so the
        /// parsed value must round-trip to the same text.
        /// </summary>
        public static bool IsValidIPv4(string text)
        {
            text = text.Trim();
            return IPAddress.TryParse(text, out IPAddress? ip)
                && ip.AddressFamily == AddressFamily.InterNetwork
                && ip.ToString() == text;
        }

        /// <summary>
        /// Returns true when the text is an IPv6 address without a zone id (e.g. "2606:4700:4700::1111").
        /// </summary>
        public static bool IsValidIPv6(string text)
        {
            text = text.Trim();
            return !text.Contains('%')
                && IPAddress.TryParse(text, out IPAddress? ip)
                && ip.AddressFamily == AddressFamily.InterNetworkV6;
        }

        /// <summary>
        /// Returns true when the text is an https DNS-over-HTTPS template (e.g. https://dns.google/dns-query).
        /// </summary>
        public static bool IsValidDoH(string text)
        {
            text = text.Trim();
            return !text.Any(char.IsWhiteSpace)
                && !text.Contains('\'')
                && Uri.TryCreate(text, UriKind.Absolute, out Uri? uri)
                && uri.Scheme == Uri.UriSchemeHttps;
        }

        /// <summary>
        /// Checks every field of an entry: DNS1 is a required IPv4 address, the rest are optional.
        /// </summary>
        public static bool IsValidEntry(DNS dns)
        {
            return IsValidIPv4(dns.DNS1())
                && (dns.DNS2() == "" || IsValidIPv4(dns.DNS2()))
                && (dns.DNS1v6() == "" || IsValidIPv6(dns.DNS1v6()))
                && (dns.DNS2v6() == "" || IsValidIPv6(dns.DNS2v6()))
                && (dns.DoH() == "" || IsValidDoH(dns.DoH()));
        }

        /// <summary>
        /// Windows 11 (build 22000) and later can use DNS over HTTPS.
        /// </summary>
        public static bool SupportsDoH => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);

        #region Settings

        private static string? GetSetting(string name)
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(SettingsKey);
            return key?.GetValue(name) as string;
        }

        private static void SetSetting(string name, string? value)
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(SettingsKey);
            if (value == null)
                key.DeleteValue(name, false);
            else
                key.SetValue(name, value);
        }

        /// <summary>
        /// Id of the adapter the user picked, or null for "automatic" (every connected adapter
        /// with an IPv4 default gateway).
        /// </summary>
        public static string? SelectedAdapterId
        {
            get => GetSetting("Adapter");
            set => SetSetting("Adapter", value);
        }

        /// <summary>
        /// "en" or "fa", or null to follow the Windows display language.
        /// </summary>
        public static string? LanguageSetting
        {
            get => GetSetting("Language");
            set => SetSetting("Language", value);
        }

        #endregion

        #region Adapters

        /// <summary>
        /// Adapters that can take an IPv4 DNS setting, connected or not.
        /// </summary>
        public static List<Adapter> ListAdapters()
        {
            List<Adapter> adapters = new List<Adapter>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    continue;

                if (!nic.Supports(NetworkInterfaceComponent.IPv4))
                    continue;

                IPv4InterfaceProperties? ipv4 = nic.GetIPProperties().GetIPv4Properties();
                if (ipv4 == null)
                    continue;

                adapters.Add(new Adapter(nic.Id, nic.Name, ipv4.Index, nic.OperationalStatus == OperationalStatus.Up));
            }

            return adapters.OrderBy(a => a.Name).ToList();
        }

        private static bool HasIPv4Gateway(NetworkInterface nic)
        {
            return nic.GetIPProperties().GatewayAddresses.Any(g =>
                g.Address.AddressFamily == AddressFamily.InterNetwork &&
                !g.Address.Equals(IPAddress.Any));
        }

        /// <summary>
        /// The adapters DNS changes apply to: the selected adapter when one is picked, otherwise
        /// the ones that are connected and carry an IPv4 default route (used for internet traffic).
        /// </summary>
        private static List<NetworkInterface> TargetInterfaces()
        {
            string? selected = SelectedAdapterId;

            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .Where(nic => nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                              nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .Where(nic => nic.Supports(NetworkInterfaceComponent.IPv4))
                .Where(nic => selected != null ? nic.Id == selected : HasIPv4Gateway(nic))
                .ToList();
        }

        private static List<int> TargetInterfaceIndexes()
        {
            return TargetInterfaces().Select(nic => nic.GetIPProperties().GetIPv4Properties().Index).ToList();
        }

        /// <summary>
        /// The DNS servers currently used by the first target adapter, or null when no adapter
        /// is connected. "Automatic" means the servers come from DHCP rather than being set by hand.
        /// </summary>
        public static CurrentDnsInfo? GetCurrentDNS()
        {
            NetworkInterface? nic = TargetInterfaces().FirstOrDefault();
            if (nic == null)
                return null;

            List<string> servers = nic.GetIPProperties().DnsAddresses
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.ToString())
                .ToList();

            // A manually set DNS is stored in the adapter's NameServer value; it is empty when
            // the servers come from DHCP.
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(
                $"SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\{nic.Id}");
            bool automatic = string.IsNullOrWhiteSpace(key?.GetValue("NameServer") as string);

            return new CurrentDnsInfo(nic.Name, automatic, servers);
        }

        #endregion

        #region Changing DNS

        private static ProcessStartInfo PowerShellStartInfo(string script)
        {
            string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

            ProcessStartInfo info = new ProcessStartInfo("powershell.exe");
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.Arguments = $"-NoProfile -NonInteractive -WindowStyle Hidden -EncodedCommand {encoded}";
            return info;
        }

        /// <summary>
        /// Runs a PowerShell script elevated and waits for it. The script is passed with
        /// -EncodedCommand so nothing in it is interpreted by a shell. When this process is
        /// already elevated the script runs directly, without a UAC prompt.
        /// </summary>
        private static DnsChangeResult RunPowerShellAsAdmin(string script)
        {
            const int ERROR_CANCELLED = 1223;

            ProcessStartInfo info = PowerShellStartInfo(script);
            if (IsAdministrator)
            {
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
            }
            else
            {
                info.UseShellExecute = true;
                info.Verb = "runas";
            }

            try
            {
                using Process? process = Process.Start(info);
                if (process == null)
                    return DnsChangeResult.Failed;

                process.WaitForExit();
                return process.ExitCode == 0 ? DnsChangeResult.Success : DnsChangeResult.Failed;
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == ERROR_CANCELLED)
            {
                return DnsChangeResult.Cancelled;
            }
            catch (Win32Exception)
            {
                return DnsChangeResult.Failed;
            }
        }

        /// <summary>
        /// Builds a script that runs <paramref name="before"/> once, then <paramref name="perInterface"/>
        /// for every target adapter ($i is the interface index), then flushes the DNS cache.
        /// Exits 1 on any error.
        /// </summary>
        private static DnsChangeResult ChangeDNS(string perInterface, string before = "")
        {
            List<int> indexes = TargetInterfaceIndexes();
            if (indexes.Count == 0)
                return DnsChangeResult.NoAdapter;

            string script =
                "$ErrorActionPreference = 'Stop'\n" +
                "try {\n" +
                before +
                $"  foreach ($i in @({string.Join(",", indexes)})) {{ {perInterface} }}\n" +
                "  Clear-DnsClientCache\n" +
                "  exit 0\n" +
                "} catch { exit 1 }";

            return RunPowerShellAsAdmin(script);
        }

        /// <summary>
        /// Sets the DNS servers (IPv4 and any IPv6) of the target adapters. When the entry has a
        /// DoH template and Windows supports it, its servers are registered for DNS over HTTPS.
        /// </summary>
        public static Task<DnsChangeResult> AddDNS(DNS dns)
        {
            // Values end up inside an elevated script, so everything is validated first and
            // addresses are written back in their normalized form.
            if (!IsValidEntry(dns))
                return Task.FromResult(DnsChangeResult.InvalidAddress);

            List<string> servers = dns.Servers().Select(s => IPAddress.Parse(s.Trim()).ToString()).ToList();
            string serverList = string.Join(",", servers.Select(s => $"'{s}'"));

            // Reset first so IPv6 servers from a previous entry do not linger.
            string perInterface =
                "Set-DnsClientServerAddress -InterfaceIndex $i -ResetServerAddresses; " +
                $"Set-DnsClientServerAddress -InterfaceIndex $i -ServerAddresses @({serverList})";

            string before = "";
            if (dns.DoH() != "" && SupportsDoH)
            {
                string template = PsQuote(dns.DoH().Trim());

                // AutoUpgrade makes Windows use DoH whenever this server is configured; falling back
                // to plain DNS keeps name resolution working where DoH is blocked.
                before = $"  foreach ($s in @({serverList})) {{\n" +
                         "    if (Get-DnsClientDohServerAddress -ServerAddress $s -ErrorAction SilentlyContinue) {\n" +
                         $"      Set-DnsClientDohServerAddress -ServerAddress $s -DohTemplate {template} -AutoUpgrade $true -AllowFallbackToUdp $true | Out-Null\n" +
                         "    } else {\n" +
                         $"      Add-DnsClientDohServerAddress -ServerAddress $s -DohTemplate {template} -AutoUpgrade $true -AllowFallbackToUdp $true | Out-Null\n" +
                         "    }\n" +
                         "  }\n";
            }

            return Task.Run(() => ChangeDNS(perInterface, before));
        }

        public static Task<DnsChangeResult> ClearDNS()
        {
            return Task.Run(() => ChangeDNS("Set-DnsClientServerAddress -InterfaceIndex $i -ResetServerAddresses"));
        }

        #endregion

        [DllImport("user32")]
        public static extern UInt32 SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        internal const int BCM_FIRST = 0x1600; //Normal button
        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button

        public static void AddShieldToButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
        }

        /// <summary>
        /// The servers added to a new database.
        /// </summary>
        public static List<DNS> PopularDNS()
        {
            return new List<DNS>
            {
                new DNS("Cloudflare", "1.1.1.1", "1.0.0.1", "2606:4700:4700::1111", "2606:4700:4700::1001", "https://cloudflare-dns.com/dns-query"),
                new DNS("Google (Public DNS)", "8.8.8.8", "8.8.4.4", "2001:4860:4860::8888", "2001:4860:4860::8844", "https://dns.google/dns-query"),
                new DNS("OpenDNS", "208.67.220.220", "208.67.222.222", "2620:119:35::35", "2620:119:53::53", "https://doh.opendns.com/dns-query"),
                new DNS("Shecan.ir", "178.22.122.100", "185.51.200.2"),
                new DNS("Electro", "78.157.42.100", "78.157.42.101"),
                new DNS("403.online", "10.202.10.202", "10.202.10.102"),
                new DNS("Begzar.ir", "185.55.226.26", "185.55.225.25"),
                new DNS("Radar.game", "10.202.10.10", "10.202.10.11"),
                new DNS("Pishgaman.net", "5.202.100.100", "5.202.100.101"),
                new DNS("Shatel.ir", "85.15.1.14", "85.15.1.15"),
                new DNS("Hostiran.net", "172.29.0.100", "172.29.2.100"),
            };
        }

        public static void AddPopularDNS()
        {
            foreach (DNS dns in PopularDNS())
                dns.Save();
        }

        #region Startup

        public static void RunAsStartup(bool agree=true)
        {
            using RegistryKey rkApp = Registry.CurrentUser.CreateSubKey(RunKey);
            if (agree) {
                rkApp.SetValue(RunValue, $"\"{Application.ExecutablePath}\"");
            }
            else
            {
                rkApp.DeleteValue(RunValue, false);
            }
        }

        public static bool CanRunAsStartup()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey);
            return key?.GetValue(RunValue) != null;
        }

        #endregion

        #region Elevated scheduled task

        // "Run as administrator" mode: a scheduled task set to run with highest privileges.
        // Windows lets the user start their own task without a UAC prompt, so the app is
        // launched through it and every later DNS change runs without asking again.

        private static (int ExitCode, string Output) RunSchtasks(string arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo("schtasks.exe", arguments);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            try
            {
                using Process? process = Process.Start(info);
                if (process == null)
                    return (-1, "");

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return (process.ExitCode, output);
            }
            catch (Win32Exception)
            {
                return (-1, "");
            }
        }

        public static bool ElevatedTaskExists()
        {
            return RunSchtasks($"/Query /TN \"{ElevatedTaskName}\"").ExitCode == 0;
        }

        public static bool ElevatedTaskRunsAtLogon()
        {
            var (exitCode, output) = RunSchtasks($"/Query /TN \"{ElevatedTaskName}\" /XML");
            return exitCode == 0 && output.Contains("<LogonTrigger");
        }

        /// <summary>
        /// Starts the app through the elevated task. Returns false when the task could not be run.
        /// </summary>
        public static bool StartElevatedTask()
        {
            return RunSchtasks($"/Run /TN \"{ElevatedTaskName}\"").ExitCode == 0;
        }

        private static string PsQuote(string text)
        {
            return "'" + text.Replace("'", "''") + "'";
        }

        /// <summary>
        /// Creates (or replaces) the elevated task, optionally starting the app at logon.
        /// Needs one UAC prompt when this process is not elevated.
        /// </summary>
        public static DnsChangeResult RegisterElevatedTask(bool atLogon)
        {
            // Use the signed-in user, not whoever approved the UAC prompt.
            string user = $"{Environment.UserDomainName}\\{Environment.UserName}";

            string script =
                "$ErrorActionPreference = 'Stop'\n" +
                "try {\n" +
                $"  $action = New-ScheduledTaskAction -Execute {PsQuote(Application.ExecutablePath)} -Argument '{FromTaskArgument}'\n" +
                $"  $principal = New-ScheduledTaskPrincipal -UserId {PsQuote(user)} -LogonType Interactive -RunLevel Highest\n" +
                // Defaults would stop the app after 3 days and skip it on battery power.
                "  $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit ([TimeSpan]::Zero) -MultipleInstances Parallel\n" +
                (atLogon
                    ? $"  $trigger = New-ScheduledTaskTrigger -AtLogOn -User {PsQuote(user)}\n" +
                      $"  Register-ScheduledTask -TaskName {PsQuote(ElevatedTaskName)} -Action $action -Principal $principal -Settings $settings -Trigger $trigger -Force | Out-Null\n"
                    : $"  Register-ScheduledTask -TaskName {PsQuote(ElevatedTaskName)} -Action $action -Principal $principal -Settings $settings -Force | Out-Null\n") +
                "  exit 0\n" +
                "} catch { exit 1 }";

            return RunPowerShellAsAdmin(script);
        }

        public static DnsChangeResult UnregisterElevatedTask()
        {
            string script =
                "$ErrorActionPreference = 'Stop'\n" +
                "try {\n" +
                $"  Unregister-ScheduledTask -TaskName {PsQuote(ElevatedTaskName)} -Confirm:$false\n" +
                "  exit 0\n" +
                "} catch { exit 1 }";

            return RunPowerShellAsAdmin(script);
        }

        #endregion
    }
}
