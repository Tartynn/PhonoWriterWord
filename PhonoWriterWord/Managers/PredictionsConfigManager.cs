using PhonoWriterWord.Sources.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Managers
{
    public static class PredictionsConfigManager
    {
        private static PredictionConfig _config = new PredictionConfig();

        public static PredictionConfig Config 
        { 
            get { return _config; } 
        }
    }
}
