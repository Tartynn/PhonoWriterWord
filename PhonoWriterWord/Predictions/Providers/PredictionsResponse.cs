using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PhonoWriterWord.Predictions.Providers
{
    [DataContract]
    public class PredictionsResponse
    {
        [DataMember(Name = "I")]
        public string Input;

        [DataMember(Name = "PS")]
        public List<string> Predictions = new List<string>();
    }
}
