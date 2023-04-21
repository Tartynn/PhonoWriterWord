//using PhonoWriterWord.Database.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PhonoWriterWord.Predictions.Predictors
//{
//    class ClassicPredictor : IPredictor
//    {
//        public ClassicPredictor()
//        { }

//        public List<PredictionValue> Predict(string input, string context, int numberOfPredictions, Language language, ParallelOptions parallelOptions)
//        {
//            List<PredictionValue> results = new List<PredictionValue>();

//            if (!parallelOptions.CancellationToken.IsCancellationRequested)
//            {
//                var words = language.Words.Where(w => w.Text.StartsWith(input, System.StringComparison.InvariantCultureIgnoreCase))
//                    .OrderByDescending(o => o.Occurrence).Take(numberOfPredictions).ToList();
//                var alternates = language.Alternatives.Where(a => a.Text.StartsWith(input, System.StringComparison.InvariantCultureIgnoreCase))
//                    .Select(w => w.Word).ToList();
//                var alternateWords = language.Words.Where(w => alternates.Contains(w.Id)).ToList();
//                words.AddRange(alternateWords);

//                foreach (Word word in words)
//                {
//                    results.Add(new PredictionValue(word.Text, word.Occurrence, PredictionTypes.CLASSIC));
//                }
//            }

//            return results;
//        }
//    }
//}
