using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Exceptions
{
    [Serializable]
    class ImageNotFoundException : ApplicationException
    {
        public ImageNotFoundException() { }
        public ImageNotFoundException(string message) { }
        public ImageNotFoundException(string message, System.Exception inner) { }

        protected ImageNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
