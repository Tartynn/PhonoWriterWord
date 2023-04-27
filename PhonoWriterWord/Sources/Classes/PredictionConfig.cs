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
        public int PredictionClassicChars { get; set; }
        public int PredictionClassicAmount { get; set; }
        public bool PredictionFuzzyActive { get; set; } = true;
        public int PredictionFuzzyAmount { get; set; }
        public bool PredictionPhoneticActive { get; set; } = true;
        //public int PredictionPhoneticAmount { get; set; }
        //public int PredictionPhoneticChars { get; set; }
        //public int PredictionPhoneticUntil {get; set; }
        public bool PredictionRelationshipActive { get; set; } = true;
        public bool PictographicActive { get; set; } = true;
        public bool HidePictureless { get; set;} = false;

        public PredictionConfig()
        {
            PredictionClassicActive = true;
            PredictionClassicAmount = 5;
            PredictionClassicChars = 3;
            PredictionFuzzyActive = true;
            PredictionFuzzyAmount = 5;
            PredictionPhoneticActive = true;
            //PredictionPhoneticAmount = 5;
            //PredictionPhoneticChars = 3;
            //PredictionPhoneticUntil = 50;
            PredictionRelationshipActive = true;
            PictographicActive = true;
            HidePictureless = false;

        }
    }
}
