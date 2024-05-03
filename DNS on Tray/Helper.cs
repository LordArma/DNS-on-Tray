using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

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

        private static void RunCMDAsAdmin(string args)
        {
            const int ERROR_CANCELLED = 1223;

            ProcessStartInfo info = new ProcessStartInfo(@"cmd.exe");
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.Verb = "runas";
            info.Arguments = args;
            try
            {
                Process.Start(info);
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == ERROR_CANCELLED)
                    return;
                else
                    throw;
            }
        }

        public static void AddDNS(string dns1, string dns2)
        {
            RunCMDAsAdmin($"/C wmic nicconfig where (IPEnabled=TRUE) call SetDNSServerSearchOrder (\"{dns1}\", \"{dns2}\")");
        }

        public static void ClearDNS()
        {
            RunCMDAsAdmin("/C wmic nicconfig where (IPEnabled=TRUE) call SetDNSServerSearchOrder ()");
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
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (agree) {    
                rkApp.SetValue("dnsontry", Application.ExecutablePath);
            }
            else
            {
                rkApp.DeleteValue("dnsontry", false);
            }
        }

        public static bool CanRunAsStartup()
        {
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
        }
    }
}
