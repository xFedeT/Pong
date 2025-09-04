using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Pong.Client.App
{
    static class ClientApplication
    {
        [STAThread]
        public static void Main()
        {
            ConfigureApplicationSettings();

            try
            {
                RunClientApplication();
            }
            catch (Exception ex)
            {
                HandleApplicationError(ex);
            }
        }

        private static void ConfigureApplicationSettings()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // High DPI awareness for modern Windows
            if (Environment.OSVersion.Version.Major >= 6)
            {
                DpiHelper.EnableDpiAwareness();
            }
        }

        private static void RunClientApplication()
        {
            using (var gameClientForm = new GameClient())
            {
                Application.Run(gameClientForm);
            }
        }

        private static void HandleApplicationError(Exception exception)
        {
            var errorMessage = $"An unexpected error occurred:\n\n{exception.Message}\n\nThe application will now close.";
            MessageBox.Show(errorMessage, "Application Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

            Console.WriteLine($"Application Error: {exception}");
        }

        static class DpiHelper
        {
            private const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3;

            [DllImport("user32.dll")]
            private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

            public static void EnableDpiAwareness()
            {
                SetProcessDpiAwarenessContext(new IntPtr(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE));
            }
        }
    }
}