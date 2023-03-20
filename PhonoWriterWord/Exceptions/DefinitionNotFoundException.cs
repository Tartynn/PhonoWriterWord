using System;
using System.Runtime.Serialization;

namespace SQLite.Persistance.DAO.SQLite
{
    [Serializable]
    internal class DefinitionNotFoundException : Exception
    {
        public DefinitionNotFoundException() {}
        public DefinitionNotFoundException(string message) : base(message) {}
        public DefinitionNotFoundException(string message, Exception innerException) : base(message, innerException) {}

        protected DefinitionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}