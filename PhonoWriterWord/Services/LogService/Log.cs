using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Services.Log
{
    public class Log
    {
        private LogService _service;
        private string _sender;

        public Log(LogService service, string sender)
        {
            _service = service;
            _sender = sender;
        }

        public void Debug(string message)
        {
            _service.Log(LogModes.DEBUG, _sender, message);
        }

        public void Debug(string message, params object[] args)
        {
            Debug(string.Format(message, args));
        }

        public void Error(string message)
        {
            _service.Log(LogModes.ERROR, _sender, message);
        }

        public void Error(string message, params object[] args)
        {
            Error(string.Format(message, args));
        }

        public void Fatal(string message)
        {
            _service.Log(LogModes.FATAL, _sender, message);
        }

        public void Fatal(string message, params object[] args)
        {
            Fatal(string.Format(message, args));
        }

        public void Info(string message)
        {
            _service.Log(LogModes.INFO, _sender, message);
        }

        public void Info(string message, params object[] args)
        {
            Info(string.Format(message, args));
        }

        public void Warn(string message)
        {
            _service.Log(LogModes.WARN, _sender, message);
        }
        public void Warn(string message, params object[] args)
        {
            Warn(string.Format(message, args));
        }
    }
}
