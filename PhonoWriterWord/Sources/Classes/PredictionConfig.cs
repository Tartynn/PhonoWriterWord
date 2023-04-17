using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Sources.Classes
{
    public class PredictionConfig
    {
        public bool PredictionClassicActive { get; set; } = true;
        public bool PredictionFuzzyActive { get; set; } = true;
        public bool PredictionAlternativeActive { get; set; } = true;
        public bool PredictionRelationshipActive { get; set; } = true;
        public bool PictographicActive { get; set; } = true;

        public PredictionConfig()
        {
            PredictionClassicActive = true;
            PredictionFuzzyActive = true;
            PredictionAlternativeActive = true;
            PredictionRelationshipActive = true;
            PictographicActive = true;

        }
    }
}
