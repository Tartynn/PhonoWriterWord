using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Exceptions
{
    [Serializable]
    internal class NoConnectionAvailableException : Exception
    {
        public NoConnectionAvailableException() { }
        public NoConnectionAvailableException(string message) : base(message) { }
        public NoConnectionAvailableException(string message, Exception innerException) : base(message, innerException) { }

        protected NoConnectionAvailableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
