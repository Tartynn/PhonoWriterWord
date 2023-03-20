using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Exceptions
{
    [Serializable]
    class WordNotFoundException : ApplicationException
    {
        public WordNotFoundException() { }
        public WordNotFoundException(string message) { }
        public WordNotFoundException(string message, System.Exception inner) { }

        protected WordNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
