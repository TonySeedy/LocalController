using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LocalController
{
    internal static class Program
    {
        // Mutex để đảm bảo chỉ có 1 instance chạy
        private static Mutex _mutex = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            const string appName = "LocalController_SingleInstanceApp";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // Ứng dụng đã được chạy, thoát
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            bool startHidden = args.Contains("--hidden") || args.Contains("--silent");
            
            Application.Run(new Form1(startHidden));
            
            // Giải phóng Mutex khi app đóng
            GC.KeepAlive(_mutex);
        }
    }
}
