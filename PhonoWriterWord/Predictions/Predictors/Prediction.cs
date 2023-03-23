
using PhonoWriterWord.Predictions.Predictors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhonoWriterWord.Predictions
{
	public abstract class Prediction
	{
		protected string _name;
		protected ThisAddIn _app;

		public string Name
		{
			get { return _name; }
		}

		public Prediction()
		{
			_app = (ThisAddIn)ThisAddIn.Current;
		}

		abstract public List<PredictionValue> Work(string input, ParallelOptions parallelOptions);

       
    }
}
