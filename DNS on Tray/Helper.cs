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
        /// Interface indexes of the adapters that are connected and carry an IPv4 default
        /// route, i.e. the ones actually used for internet traffic.
        /// </summary>
        private static List<int> ActiveInterfaceIndexes()
        {
            List<int> indexes = new List<int>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    continue;

                if (!nic.Supports(NetworkInterfaceComponent.IPv4))
                    continue;

                IPInterfaceProperties props = nic.GetIPProperties();
                bool hasGateway = props.GatewayAddresses.Any(g =>
                    g.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !g.Address.Equals(IPAddress.Any));

                if (hasGateway)
                    indexes.Add(props.GetIPv4Properties().Index);
            }

            return indexes;
        }

        /// <summary>
        /// Runs a PowerShell script elevated and waits for it. The script is passed with
        /// -EncodedCommand so nothing in it is interpreted by a shell.
        /// </summary>
        private static DnsChangeResult RunPowerShellAsAdmin(string script)
        {
            const int ERROR_CANCELLED = 1223;

            string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

            ProcessStartInfo info = new ProcessStartInfo("powershell.exe");
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.Verb = "runas";
            info.Arguments = $"-NoProfile -NonInteractive -WindowStyle Hidden -EncodedCommand {encoded}";

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
        /// Builds a script that runs <paramref name="perInterface"/> for every active adapter
        /// ($i is the interface index), then flushes the DNS cache. Exits 1 on any error.
        /// </summary>
        private static DnsChangeResult ChangeDNS(string perInterface)
        {
            List<int> indexes = ActiveInterfaceIndexes();
            if (indexes.Count == 0)
                return DnsChangeResult.NoAdapter;

            string script =
                "$ErrorActionPreference = 'Stop'\n" +
                "try {\n" +
                $"  foreach ($i in @({string.Join(",", indexes)})) {{ {perInterface} }}\n" +
                "  Clear-DnsClientCache\n" +
                "  exit 0\n" +
                "} catch { exit 1 }";

            return RunPowerShellAsAdmin(script);
        }

        public static Task<DnsChangeResult> AddDNS(string dns1, string dns2)
        {
            dns1 = dns1.Trim();
            dns2 = dns2.Trim();

            // Addresses end up inside an elevated script, so only strict IPv4 text is allowed.
            if (!IsValidIPv4(dns1) || !IsValidIPv4(dns2))
                return Task.FromResult(DnsChangeResult.InvalidAddress);

            return Task.Run(() => ChangeDNS(
                $"Set-DnsClientServerAddress -InterfaceIndex $i -ServerAddresses @('{dns1}','{dns2}')"));
        }

        public static Task<DnsChangeResult> ClearDNS()
        {
            return Task.Run(() => ChangeDNS("Set-DnsClientServerAddress -InterfaceIndex $i -ResetServerAddresses"));
        }

        [DllImport("user32")]
        public static extern UInt32 SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        internal const int BCM_FIRST = 0x1600; //Normal button
        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button

        public static void AddShieldToButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
        }

        public static void AddPopularDNS()
        {
            DNS dns;

            dns = new DNS("Cloudflare", "1.1.1.1", "1.0.0.1");
            dns.Save();

            dns = new DNS("Google (Public DNS)", "8.8.8.8", "8.8.4.4");
            dns.Save();

            dns = new DNS("OpenDNS", "208.67.220.220", "208.67.222.222");
            dns.Save();

            dns = new DNS("Shecan.ir", "178.22.122.100", "185.51.200.2");
            dns.Save();

            dns = new DNS("Electro", "78.157.42.100", "78.157.42.101");
            dns.Save();

            dns = new DNS("403.online", "10.202.10.202", "10.202.10.102");
            dns.Save();

            dns = new DNS("Begzar.ir", "185.55.226.26", "185.55.225.25");
            dns.Save();

            dns = new DNS("Radar.game", "10.202.10.10", "10.202.10.11");
            dns.Save();

            dns = new DNS("Pishgaman.net", "5.202.100.100", "5.202.100.101");
            dns.Save();

            dns = new DNS("Shatel.ir", "85.15.1.14", "85.15.1.15");
            dns.Save();

            dns = new DNS("Hostiran.net", "172.29.0.100", "172.29.2.100");
            dns.Save();
        }

        public static void RunAsStartup(bool agree=true)
        {
#pragma warning disable CS8600
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
#pragma warning disable CS8602
            if (agree) {    
                rkApp.SetValue("dnsontry", Application.ExecutablePath);
            }
            else
            {
                rkApp.DeleteValue("dnsontry", false);
            }
#pragma warning restore CS8602
#pragma warning restore CS8600
        }

        public static bool CanRunAsStartup()
        {
#pragma warning disable CS8600
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
            {
                if (key != null)
                {
                    Object o = key.GetValue("dnsontry");
                    if (o != null)
                    {
                        return true;
                    }

                    return false;
                }
                else
                {
                    return false;
                }
            }
#pragma warning restore CS8600
        }
    }
}
