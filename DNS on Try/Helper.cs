using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DNS_on_Try
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
            RunCMDAsAdmin($"/C netsh interface ipv4 add dnsserver \"Wi-Fi\" address={dns1} index=1 & netsh interface ipv4 add dnsserver \"Wi-Fi\" address={dns2} index=2");
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
            b.FlatStyle = FlatStyle.System;
            SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
        }
    }
}
