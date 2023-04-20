using PhonoWriterWord.Database;
using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Managers;
using PhonoWriterWord.Sources.Classes;
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
            PredictionConfig config = PredictionsConfigManager.Config;

            if (!config.PredictionRelationshipActive)
            {
                return results;
            }

            if (parallelOptions.CancellationToken.IsCancellationRequested)
				return results;

			var fr = new Database.Models.Language(1, "fr");
			Word word = fr.Words.Find(m => m.Text.ToLower() == input.ToLower());//_app.LanguagesManager.CurrentLanguage.Words.Find(m => m.Text.ToLower() == input.ToLower());
			if (word == null)
				return results;

			List<Pair> pairs;
			var dbC = ThisAddIn.Current.DatabaseController; // call the DatabaseController variable instantiated by the current session of the Add-Ins
			pairs = dbC.PairsController.ResearchByFirstWord(word);
			System.Diagnostics.Debug.WriteLine("HERE WE AAAAAAAARRRRRREEEEEEEEEEEE - Predic Relationship");
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
