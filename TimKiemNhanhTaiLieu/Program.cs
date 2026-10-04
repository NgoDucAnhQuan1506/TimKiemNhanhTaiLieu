using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TimKiemNhanhTaiLieu
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Try to make the process DPI aware so WinForms renders crisply on high-DPI displays.
            // This must be called before any windows are created.
            try
            {
                SetProcessDPIAware();
            }
            catch { /* ignore if not supported */ }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
