using System;
using System.IO;
using System.Text;

namespace LocalController.Logger
{
    public class FileLogger
    {
        private readonly string _logsDirectory;
        private readonly object _lock = new object();

        public event Action<string> Logged;

        public FileLogger(string baseDirectory)
        {
            _logsDirectory = Path.Combine(baseDirectory, "logs");
            Directory.CreateDirectory(_logsDirectory);
        }

        public void Log(string message)
        {
            var line = string.Format("{0:yyyy-MM-dd HH:mm:ss} {1}", DateTime.Now, message);
            var filePath = Path.Combine(_logsDirectory, DateTime.Now.ToString("yyyy-MM-dd") + ".log");

            lock (_lock)
            {
                File.AppendAllText(filePath, line + Environment.NewLine, Encoding.UTF8);
            }

            var handler = Logged;
            if (handler != null)
            {
                handler(line);
            }
        }
    }
}
