﻿using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Predictions;
using PhonoWriterWord.Sources.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Services
{
	public class PredictionsService
	{
		#region Variables

		private ThisAddIn _app;

        private Configuration _config;
        //private Log _log;

        private PredictionsRequest _request;

		#endregion

		#region Contructors

		public PredictionsService()
		{
			_app = ThisAddIn.Current;
			//_log = new Log(_app.LogService, GetType().Name);
			_config = (Configuration)_app.Configuration;
		}

		#endregion

		#region Methods

		public List<PredictionValue> Request(List<Prediction> predictions, string input)
		{
			if (_request != null)
				_request.Cancel();

			System.Diagnostics.Debug.WriteLine("PredictionsService - List<PredictionValue> Request(list predi, string input) - [input : " + input +"]");

			_request = new PredictionsRequest(predictions, input.Trim());

			//var r= _request.Run();
			//foreach(var w in r)
   //         {
			//	System.Diagnostics.Debug.WriteLine("the return can be : " + w.Prediction);

			//}

			return _request.Run();
		}

		public void UpdatePair(Language language, string first, string second, int occurrence = 1)
		{
			if (first == second)
				return;

			// Increment occurrence.
			Word wordFirst = language.Words.Find(m => m.Text == first.Trim());
			Word wordSecond = language.Words.Find(m => m.Text == second.Trim());

			if (wordFirst == null || wordSecond == null)
				return;

			// Pair exists ? Update it.
			Pair pair = _app.DatabaseController.PairsController.ResearchByWords(wordFirst, wordSecond);
			if (pair != null)
			{
				pair.Occurrence += occurrence;
				pair.IsUpdated = true;
				_app.DatabaseController.PairsController.Update(pair);
				System.Diagnostics.Debug.WriteLine("UpdatePair -> updated '{0}' <-> '{1}' pair (Occurrence : {2})", first, second, pair.Occurrence);
				return; // Pass the creation part.
			}

			// Pair doesn't exist ? Create it.
			pair = new Pair(wordFirst, wordSecond);
			pair.IsUpdated = true;
			_app.DatabaseController.PairsController.Create(pair);
			System.Diagnostics.Debug.WriteLine("UpdatePair -> created '{0}' <-> '{1}' pair", first, second);
		}

		#endregion
	}
}
