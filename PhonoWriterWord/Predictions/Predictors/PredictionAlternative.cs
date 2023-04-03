using PhonoWriterWord.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Predictions
{
	class PredictionAlternative : Prediction
	{
		public PredictionAlternative()
		{
			_name = "Alternative";
		}

		public override List<PredictionValue> Work(string input, ParallelOptions parallelOptions)
		{
			List<PredictionValue> results = new List<PredictionValue>();

			//if (!_app.Configuration.ClassicPredictionActivated)
			//	return results;

			if (parallelOptions.CancellationToken.IsCancellationRequested)
				return results;

			var numberOfPrediction = 1;//_app.Configuration.ClassicPredictionsNumber;

			//var fr = new Database.Models.Language(1, "fr");
			var language = Globals.ThisAddIn.LanguagesManager.CurrentLanguage;

            //var alternatives = /*_app.LanguagesManager.CurrentLanguage*/fr.Alternatives.Where(a => a.Text.Equals(input, System.StringComparison.InvariantCultureIgnoreCase)).Take(numberOfPrediction).ToList();
			var alternatives = language.Alternatives.Where(a => a.Text.Equals(input, System.StringComparison.InvariantCultureIgnoreCase)).Take(numberOfPrediction).ToList();
            if (input.Length >= 4)
			{
                //alternatives.AddRange(/*_app.LanguagesManager.CurrentLanguage*/fr.Alternatives.Where(a => a.Text.StartsWith(input, System.StringComparison.InvariantCultureIgnoreCase)).Take(numberOfPrediction).ToList());
                alternatives.AddRange(language.Alternatives.Where(a => a.Text.StartsWith(input, System.StringComparison.InvariantCultureIgnoreCase)).Take(numberOfPrediction).ToList());
            }
			var alternateWordsIds = alternatives.Select(a => a.Word).ToArray();
			//var alternateWords = /*_app.LanguagesManager.CurrentLanguage*/fr.Words.Where(w => alternateWordsIds.Contains(w.Id));
            var alternateWords = language.Words.Where(w => alternateWordsIds.Contains(w.Id));

            foreach (Word word in alternateWords)
			{
				PredictionValue pv = new PredictionValue();
				pv.Prediction = word.Text;
				pv.Value = word.Occurrence;
				pv.Type = PredictionType.ALTERNATIVE;
				results.Add(pv);
			}

			return results;
		}
	}
}
