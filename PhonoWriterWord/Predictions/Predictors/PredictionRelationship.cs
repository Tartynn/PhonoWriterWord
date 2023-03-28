using PhonoWriterWord.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Predictions.Predictors
{
	class PredictionRelationship : Prediction
	{
		public PredictionRelationship()
		{
			_name = "Relationship";
		}

		public override List<PredictionValue> Work(string input, ParallelOptions parallelOptions)
		{
			List<PredictionValue> results = new List<PredictionValue>();

			if (parallelOptions.CancellationToken.IsCancellationRequested)
				return results;

			Word word = _app.LanguagesManager.CurrentLanguage.Words.Find(m => m.Text.ToLower() == input.ToLower());
			if (word == null)
				return results;

			List<Pair> pairs;
			pairs = _app.DatabaseController.PairsController.ResearchByFirstWord(word);
			pairs = pairs.OrderByDescending(o => o.Occurrence).Take(10).ToList();

			foreach (Pair pair in pairs)
			{
				Word w = _app.DatabaseController.WordsController.Research(pair.NextWord);
				PredictionValue pv = new PredictionValue()
				{
					Prediction = w.Text,
					Value = w.Occurrence,
					Type = PredictionType.RELATIONSHIP
				};
				results.Add(pv);
			}

			return results;
		}
	}
}
