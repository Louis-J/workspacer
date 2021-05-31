using System;
using System.Threading;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;

namespace workspacer
{

    class Program
    {
        private static workspacer _app;
        private static Logger _logger = Logger.Create();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _app = new workspacer();

            Thread.GetDomain().UnhandledException += ((s, e) =>
                {
                    _logger.Fatal((Exception) e.ExceptionObject, "exception occurred, quiting workspacer: " + ((Exception) e.ExceptionObject).ToString());
                    _app.QuitWithException((Exception) e.ExceptionObject);
                });
            _app.Start();
        }
    }
}
