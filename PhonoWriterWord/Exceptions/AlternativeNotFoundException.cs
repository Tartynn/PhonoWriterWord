using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Exceptions
{
    [Serializable]
    class AlternativeNotFoundException : ApplicationException
    {
        public AlternativeNotFoundException() { }
        public AlternativeNotFoundException(string message) { }
        public AlternativeNotFoundException(string message, System.Exception inner) { }

        protected AlternativeNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
