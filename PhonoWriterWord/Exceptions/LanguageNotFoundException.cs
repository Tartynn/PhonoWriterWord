using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Exceptions
{
    [Serializable]
    class LanguageNotFoundException : ApplicationException
    {
        public LanguageNotFoundException() { }
        public LanguageNotFoundException(string message) { }
        public LanguageNotFoundException(string message, System.Exception inner) { }

        protected LanguageNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
