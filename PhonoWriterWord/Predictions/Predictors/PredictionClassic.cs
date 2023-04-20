﻿using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Managers;
using PhonoWriterWord.Sources.Classes;
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

		public override List<PredictionValue> Work(string input, ParallelOptions parallelOptions)
		{
			List<PredictionValue> results = new List<PredictionValue>();
			PredictionConfig config = PredictionsConfigManager.Config;

            //if (!_app.Configuration.ClassicPredictionActivated)
            //    return results;


			if (input.Length < config.PredictionClassicChars)
				return results;

			if (parallelOptions.CancellationToken.IsCancellationRequested)
				return results;

			if (!config.PredictionClassicActive)
				return results;

			var numberOfPrediction = config.PredictionClassicAmount;
            System.Diagnostics.Debug.WriteLine("Amount of classical predictions " + config.PredictionClassicAmount);
            //var numberOfPrediction = 9;//_app.Configuration.ClassicPredictionsNumber;
            //var fr = new Database.Models.Language(1, "fr");
            var language = Globals.ThisAddIn.LanguagesManager.CurrentLanguage;
            //var words = /*_app.LanguagesManager.CurrentLanguage*/fr.Words.Where(w => w.Text.StartsWith(input, System.StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(o => o.Occurrence).Take(numberOfPrediction).ToList();
			var words = language.Words.Where(w => w.Text.StartsWith(input, System.StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(o => o.Occurrence).Take(numberOfPrediction).ToList();

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
