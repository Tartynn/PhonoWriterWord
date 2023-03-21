using PhonoWriterWord.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Predictions.Predictors
{
    class PredictionClassic : Prediction
	{
		public PredictionClassic()
		{
			_name = "Classic";
		}

		public override List<PredictionValue> Work(string input/*, ParallelOptions parallelOptions*/)
		{
			List<PredictionValue> results = new List<PredictionValue>();

            //if (!_app.Configuration.ClassicPredictionActivated)
            //    return results;

            //if (input.Length < _app.Configuration.ClassicPredictionsMinCharNumber)
            //    return results;

            //if (parallelOptions.CancellationToken.IsCancellationRequested)
            //    return results;

            var numberOfPrediction = /*_app.Configuration.ClassicPredictionsNumber*/9;
			var fr = new Database.Models.Language(1, "fr");
			var words = /*_app.LanguagesManager.CurrentLanguage*/fr.Words.Where(w => w.Text.StartsWith(input, System.StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(o => o.Occurrence).Take(numberOfPrediction).ToList();

			foreach (Word word in words)
			{
				PredictionValue pv = new PredictionValue(word.Text,word.Occurrence, PredictionType.CLASSIC);
				/*pv.Prediction = word.Text;
				pv.Value = word.Occurrence;
				pv.Type = PredictionTypes.CLASSIC;*/
				results.Add(pv);
			}

			return results;
		}
	}
}