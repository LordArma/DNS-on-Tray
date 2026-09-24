using static DNS_on_Tray.Helper;

namespace DNS_on_Tray
{
    internal static class Program
    {
        private const string MutexName = "systemontray123";
        private const string ShowEventName = "DNSonTray.ShowWindow";

        private static Mutex? mutex;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // In "Run as administrator" mode, start through the elevated task instead. The
            // argument check stops a loop when the task itself cannot elevate (standard users).
            if (!args.Contains(FromTaskArgument) && !IsAdministrator && ElevatedTaskExists() && StartElevatedTask())
                return;

            bool createdNew;
            try
            {
                mutex = new Mutex(true, MutexName, out createdNew);
            }
            catch (UnauthorizedAccessException)
            {
                // Held by an instance running at a higher privilege level.
                createdNew = false;
            }

            if (!createdNew)
            {
                SignalRunningInstance();
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            using var showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);
            var form = new frmMain();

            // Another launch asks this instance to show its window.
            ThreadPool.RegisterWaitForSingleObject(showEvent, (_, _) =>
            {
                if (form.IsHandleCreated)
                    form.BeginInvoke(form.ShowMainWindow);
            }, null, Timeout.Infinite, false);

            Application.Run(form);
            ReleaseSingleInstance();
        }

        private static void SignalRunningInstance()
        {
            try
            {
                if (EventWaitHandle.TryOpenExisting(ShowEventName, out EventWaitHandle? showEvent))
                {
                    showEvent.Set();
                    showEvent.Dispose();
                }
            }
            catch (UnauthorizedAccessException)
            {
                // The running instance is elevated and this one is not; nothing more to do.
            }
        }

        /// <summary>
        /// Lets a new instance start while this one is shutting down (used when restarting elevated).
        /// </summary>
        public static void ReleaseSingleInstance()
        {
            if (mutex == null)
                return;

            mutex.ReleaseMutex();
            mutex.Dispose();
            mutex = null;
        }
    }
}
