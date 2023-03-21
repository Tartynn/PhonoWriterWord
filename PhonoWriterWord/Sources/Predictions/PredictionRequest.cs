﻿using PhonoWriterWord.Predictions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PhonoWriterWord.Sources.Predictions
{
	public enum PredictionTypes
	{
		CLASSIC,
		PHONETIC,
		FUZZY,
		RELATIONSHIP,
		ALTERNATIVE
	}

	public class PredictionValue
	{
		public string Prediction;
		public float Value;
		public PredictionTypes Type;
	}

	class PredictionsRequest
	{
		#region Variables

		private ThisAddIn _app;
		//private Log _log;

		private string _input;
		private List<Prediction> _predictions;

		private CancellationTokenSource _cts;
		private ParallelOptions _parallelOptions;

		#endregion

		#region Properties

		public string Input => _input;

		#endregion

		#region Constructors

		public PredictionsRequest(List<Prediction> predictions, string input)
		{
			_app = ThisAddIn.Current;
			//_log = new Log(_app.LogService, GetType().Name);
			_predictions = predictions;
			_input = input;
		}

		#endregion

		#region Methods

		public void Cancel()
		{
			if (_cts != null && !_cts.IsCancellationRequested)
				_cts.Cancel();
		}

		public List<PredictionValue> Run()
		{
			_cts = new CancellationTokenSource();
			_parallelOptions = new ParallelOptions();
			_parallelOptions.CancellationToken = _cts.Token;
			_parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount;

			ConcurrentBag<PredictionValue> results = new ConcurrentBag<PredictionValue>();

			try
			{
				Parallel.ForEach(_predictions, _parallelOptions, (prediction) =>
				{
					//_log.Debug("Starting {0} prediction for '{1}'", prediction.Name, _input);

					if (_parallelOptions.CancellationToken.IsCancellationRequested)
						return;

					var predictionResults = prediction.Work(_input, _parallelOptions);

					if (_parallelOptions.CancellationToken.IsCancellationRequested)
						return;

					//_log.Debug("Ended {0} prediction", prediction.Name);

					//predictionResults.ForEach(x => results.Add(x));
				});
			}
			catch // (OperationCanceledException e)
			{
				//_log.Debug("Cancelled prediction for '{0}'", _input);

				// Clear the results.
				PredictionValue result;
				while (results.TryTake(out result)) ;
			}
			finally
			{
				_cts.Dispose();
				_cts = null;
			}

			return new List<PredictionValue>(results.ToArray());
		}

		#endregion
	}
}