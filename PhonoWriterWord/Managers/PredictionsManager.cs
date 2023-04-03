using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Predictions;
using PhonoWriterWord.Predictions.Predictors;
using PhonoWriterWord.Services.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhonoWriterWord.Managers
{
    public class PredictionsManager
    {
        #region Variables

        private ThisAddIn _app;
        private Log _log;

        private string _lastInput;
        private string _currentInput;

        private List<Prediction> _predictions;
        private PredictionClassic _predictionClassic;
        private PredictionAlternative _predictionAlternative;
        private PredictionFuzzy _predictionFuzzy;
        //private PredictionPhonetic _predictionPhonetic;
        private PredictionRelationship _predictionRelationship;

        public enum PredictionsEnum
        {
            Classic,
            Fuzzy,
            Phonetic,
            Relationship,
            Alternative
        }

        // Events
        public event EventHandler<PredictionsFoundArgs> PredictionsFound;

        #endregion

        #region Constructors

        public PredictionsManager()
        {
            _app = (ThisAddIn)ThisAddIn.Current;
           // _log = new Log(_app.LogService, GetType().Name);

            _lastInput = string.Empty;
            _currentInput = string.Empty;
            _predictions = new List<Prediction>();
        }

        #endregion

        #region Methods

        public void DisablePrediction(PredictionsEnum typeOfPrediction)
        {
            _predictions.Remove(GetPrediction(typeOfPrediction));
        }

        public void EnablePrediction(PredictionsEnum typeOfPrediction)
        {
            _predictions.Add(GetPrediction(typeOfPrediction));
        }

        /// <summary>
        /// Filters and order the predictions.
        /// 
        /// We go like this : 
        /// 
        /// 1) We count the number of occurencies of each prediction.
        /// 2) Make three lists:
        ///     2.1) Predictions that appear more than one time.
        ///     2.2) Predictions that appear only one time.
        ///     2.3) Predictions that doesn't exist in the language.
        /// 3) Sort them by occurrence (except 2.3).
        /// 4) Join 2.1 and 2.2 (lists with existing words).
        /// 5) Remove pictureless existing words if necessary.
        /// 6) Join the three lists (and ignore 2.3 if 5 is applied
        ///    -> non existing words = pictureless !).
        /// 7) Compare input's hash to predictions hash and put the most
        ///    similar word on top.
        /// 
        /// </summary>
        /// <returns>Filtered results</returns>
        /// <param name="predictionsStrings">Predictions</param>
        /// <param name="input">Input</param>
        public List<string> Filter(List<Predictions.PredictionValue> predictions, string input)
        {
            var language = _app.LanguagesManager.CurrentLanguage;
            List<Word> words = language.Words;

            // Transform prediction values to strings and separate "Alternative" prediction values
            List<Predictions.PredictionValue> alternatives = predictions.FindAll(pv => pv.Type == PredictionType.ALTERNATIVE);
            predictions.RemoveAll(pv => pv.Type == PredictionType.ALTERNATIVE);
            List<string> predictionsStrings = predictions.Select(s => s.Prediction).Distinct().ToList();

            // Regroup by occurencies (high = > 1 occurences, low = 1 occurency).
            List<string> highs = predictionsStrings.Where(p => predictionsStrings.Count(c => c == p) > 1).GroupBy(s => s).OrderByDescending(g => predictionsStrings.Count(p => p == g.Key)).Select(g => g.Key).ToList();
            List<string> lows = predictionsStrings.Where(p => predictionsStrings.Count(c => c == p) == 1).GroupBy(s => s).Select(g => g.Key).ToList();
            List<string> inexistants = new List<string>();

            // Sort high ranked first, exclude inexistants.
            List<Word> sortedHigh = new List<Word>();
            foreach (var high in highs)
            {
                var word = words.Find(f => f.Text == high);
                if (word != null)
                    sortedHigh.Add(word);
                else
                    inexistants.Add(high);
            }
            sortedHigh = sortedHigh.OrderByDescending(o => o.Occurrence).ToList();

            // Sort low ranked, exclude inexistants.
            List<Word> sortedLow = new List<Word>();
            foreach (var low in lows)
            {
                var word = words.Find(f => f.Text == low);
                if (word != null)
                    sortedLow.Add(word);
                else
                    inexistants.Add(low);
            }
            sortedLow = sortedLow.OrderByDescending(o => o.Occurrence).ToList();


            // Join existant words lists.
            sortedHigh.AddRange(sortedLow);

            // Don't show pictureless words if option is activated.
            /*if (_configuration.PictographicHidePictureless)
            {
                for (int i = 0; i < sortedHigh.Count; i++)
                {
                    Word w = sortedHigh[i];
                    if (w.Image == 0)
                    {
                        sortedHigh.RemoveAt(i);
                        i--;
                    }
                }
            }*/


            // Transform List<Word> to List<string>.
            predictionsStrings = sortedHigh.Select(s => s.Text).ToList();

            // Add inexistants if necessary.
            //if (!_configuration.PictographicHidePictureless)
            predictionsStrings.AddRange(inexistants);

            //// TODO : fix this
            //// Prevent overflow of predictions (rare bug) until it's fixed.
            //int max = _app.Configuration.ClassicPredictionsNumber + _app.Configuration.PhoneticPredictionNumber + _app.Configuration.FuzzyPredictionNumber;
            //if (predictionsStrings.Count > max)
            //    predictionsStrings.RemoveRange(max, predictionsStrings.Count - max);

            //// Put the most similar hashed word on top.
            //var inputHash = Algorithms.Splitter(input, (LanguagesEnum)_app.Configuration.Language);
            //for (int i = 0; i < predictionsStrings.Count; i++)
            //{
            //    string prediction = predictionsStrings[i];
            //    var predictionHash = Algorithms.Splitter(predictionsStrings[i], (LanguagesEnum)_app.Configuration.Language);
            //    if (inputHash == predictionHash)
            //    {
            //        predictionsStrings.RemoveAt(i);
            //        predictionsStrings.Insert(0, prediction); // Put word on top.
            //        break;
            //    }
            //}

            // Add alternatives on top of all
            predictionsStrings.InsertRange(0, alternatives.Select(s => s.Prediction).Distinct().ToList());

            return predictionsStrings;
        }

        private Prediction GetPrediction(PredictionsEnum typeOfPrediction)
        {
            Prediction prediction = null;
            switch (typeOfPrediction)
            {
                case PredictionsEnum.Classic: prediction = _predictionClassic; break;
                case PredictionsEnum.Alternative: prediction = _predictionAlternative; break;
                case PredictionsEnum.Fuzzy: prediction = _predictionFuzzy; break;
                //case PredictionsEnum.Phonetic: prediction = _predictionPhonetic; break;
                case PredictionsEnum.Relationship: prediction = _predictionRelationship; break;
            }

            return prediction;
        }

        public void Initialize()
        {
            // Initialize prediction systems.
            _predictionClassic = new PredictionClassic();
            _predictionAlternative = new PredictionAlternative();
            _predictionFuzzy = new PredictionFuzzy();
            //_predictionPhonetic = new PredictionPhonetic();
            _predictionRelationship = new PredictionRelationship();

            _predictions.Clear();

            _predictions.Add(_predictionClassic);
            _predictions.Add(_predictionAlternative);
            _predictions.Add(_predictionFuzzy);
            //if (_app.Configuration.PhoneticPredictionActivated && _app.EnginesManager.HasEngines && _predictionPhonetic != null)
            //_predictions.Add(_predictionPhonetic);
            _predictions.Add(_predictionRelationship);
        }

        public void Request(string input)
        {
            System.Diagnostics.Debug.WriteLine("PredictionsManager.cs - Request with input");
            Request(_predictions, input);
        }

        public void Request(List<Prediction> predictions, string input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input))
            {

                _lastInput = string.Empty;

                PredictionsFound?.Invoke(this, new PredictionsFoundArgs(input, new List<PredictionValue>()));
                return;
            }

            System.Diagnostics.Debug.WriteLine("PredictionsManager.cs - Request with List predi + input");

            _lastInput = input;

            // Launch request.

            Task.Run(() =>
            {
                //_log.Debug("TextProviderManager_TextFound [text : '{0}', lastInput : '{1}']", input, _lastInput);

                Thread.Sleep(100);

                if (input != _lastInput) return; // Exit task if a new one has been called.

                List<PredictionValue> results = _app.PredictionsService.Request(predictions, input);
                System.Diagnostics.Debug.WriteLine("PredictionsManager.cs - Request sent with predictions + input");

                PredictionsFound?.Invoke(this, new PredictionsFoundArgs(input, results));
                System.Diagnostics.Debug.WriteLine("PredictionsManager.cs - END of request");
            });
        }

        public void RequestRelationshipsAsync(string input)
        {
            Request(new List<Prediction>() { _predictionRelationship }, input);
        }

        public List<PredictionValue> RequestRelationships(string input)
        {
            return _app.PredictionsService.Request(new List<Prediction>() { _predictionRelationship }, input);
        }

        #endregion
    }

    public class PredictionSelectedArg
    {
        string _input;
        string _prediction;

        public string Input
        {
            get { return _input; }
        }

        public string Prediction
        {
            get { return _prediction; }
        }

        public PredictionSelectedArg(string input, string prediction)
        {
            _input = input;
            _prediction = prediction;
        }
    }

    public class PredictionsFoundArgs
    {
        string _input;
        List<PredictionValue> _predictions;

        public string Input
        {
            get { return _input; }
        }

        public List<PredictionValue> Predictions
        {
            get { return _predictions; }
        }

        public PredictionsFoundArgs(string input, List<PredictionValue> predictions)
        {
            _input = input;
            _predictions = predictions;
        }
    }
}
