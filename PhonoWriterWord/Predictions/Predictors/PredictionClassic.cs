using PhonoWriterWord.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Predictions
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

<<<<<<< HEAD
			if (input.Length < 3)//_app.Configuration.ClassicPredictionsMinCharNumber)
				return results;

			if (parallelOptions.CancellationToken.IsCancellationRequested)
				return results;

			var numberOfPrediction = 9;//_app.Configuration.ClassicPredictionsNumber;
=======
            //if (input.Length < _app.Configuration.ClassicPredictionsMinCharNumber)
            //    return results;

            //if (parallelOptions.CancellationToken.IsCancellationRequested)
            //    return results;

            var numberOfPrediction = /*_app.Configuration.ClassicPredictionsNumber*/9;
>>>>>>> efbcefc78007c1d4d21d7b66123c400ed3b46a57
			var fr = new Database.Models.Language(1, "fr");
			var words = /*_app.LanguagesManager.CurrentLanguage*/fr.Words.Where(w => w.Text.StartsWith(input, System.StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(o => o.Occurrence).Take(numberOfPrediction).ToList();

			foreach (Word word in words)
			{
<<<<<<< HEAD
				PredictionValue pv = new PredictionValue();
				pv.Prediction = word.Text;
				pv.Value = word.Occurrence;
				pv.Type = PredictionType.CLASSIC;
=======
				PredictionValue pv = new PredictionValue(word.Text,word.Occurrence, PredictionType.CLASSIC);
				/*pv.Prediction = word.Text;
				pv.Value = word.Occurrence;
				pv.Type = PredictionTypes.CLASSIC;*/
>>>>>>> efbcefc78007c1d4d21d7b66123c400ed3b46a57
				results.Add(pv);
			}

			return results;
		}
	}
}
