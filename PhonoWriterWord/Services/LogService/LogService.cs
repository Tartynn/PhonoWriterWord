using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Services.Log
{
    [Flags]
    public enum LogModes
    {
        NONE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL,
        STANDARD = INFO | ERROR | FATAL,
        ALL = DEBUG | INFO | WARN | ERROR | FATAL
    }

    public class LogService : IDisposable
    {
        #region Variables

        // Fields
        private string _path;
        private int _numberOfLogs;
        private LogModes _mode = LogModes.STANDARD;
        private StreamWriter _writer;

        // Constants
        private readonly string EXTENSION = ".log";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        public LogModes Mode
        {
            get { return _mode; }
            set
            {
                Log(LogModes.INFO, GetType().Name, "Mode set as " + value.ToString());
                _mode = value;
            }
        }

        #endregion

        #region Constructors

        public LogService(string path, int numberOfLogs)
        {
            _path = path;
            _numberOfLogs = numberOfLogs;

            RotateLogs();
            Open();
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            if (_writer == null)
                return;

            _writer.Close();
            _writer.Dispose();
            _writer = null;
        }

        public void Log(LogModes type, string sender, string message)
        {
            if (!_mode.HasFlag(type))
                return;

            string output = string.Format("{0} [{1}] {2}::{3}",
                DateTime.Now.ToString("HH:mm:ss"),
                type.ToString(),
                sender,
                message
                );

            Console.WriteLine(output);

            // Avoid writing when the writer is already disposed (thread problem).
            // I guess we have no choice...
            if (_writer != null)
            {
                _writer.WriteLine(output);
                _writer.Flush();
            }
        }

        public void Open()
        {
            try
            {
                _writer = File.AppendText(_path);
            }
            catch
            {
                Log(LogModes.ERROR, GetType().Name, "Cannot write log on disk. Please delete " + _path);
                Dispose();
            }
        }

        private void RotateLogs()
        {
            string file = _path.Replace(EXTENSION, "." + (_numberOfLogs - 1) + EXTENSION); // Oldest log.
            string dest = string.Empty;

            try
            {
                // Remove oldest log.
                if (File.Exists(file))
                    File.Delete(file);

                // For each log until the newest...
                for (int i = _numberOfLogs - 2; i >= 0; i--)
                {
                    dest = file;
                    file = _path.Replace(EXTENSION, "." + i + EXTENSION); // mylog.I.log -> mylog.(I + 1).log
                    if (File.Exists(file))
                        File.Move(file, dest);
                }

                // Move the newest to second (mylog.log -> mylog.0.log).
                if (File.Exists(_path))
                    File.Move(_path, file);
            }
            catch
            {
                Log(LogModes.ERROR, GetType().Name, "Error while logs rotation.");
            }
        }

        #endregion
    }
}
