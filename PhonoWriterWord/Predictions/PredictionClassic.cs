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

		public override List<PredictionValue> Work(string input, ParallelOptions parallelOptions)
		{
			List<PredictionValue> results = new List<PredictionValue>();

			//if (!_app.Configuration.ClassicPredictionActivated)
			//	return results;

			if (input.Length < 3)//_app.Configuration.ClassicPredictionsMinCharNumber)
				return results;

			if (parallelOptions.CancellationToken.IsCancellationRequested)
				return results;

			var numberOfPrediction = 1;//_app.Configuration.ClassicPredictionsNumber;
			var words = _app.LanguagesManager.CurrentLanguage.Words.Where(w => w.Text.StartsWith(input, System.StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(o => o.Occurrence).Take(numberOfPrediction).ToList();

			foreach (Word word in words)
			{
				PredictionValue pv = new PredictionValue();
				pv.Prediction = word.Text;
				pv.Value = word.Occurrence;
				pv.Type = PredictionType.CLASSIC;
				results.Add(pv);
			}

			return results;
		}
	}
}
