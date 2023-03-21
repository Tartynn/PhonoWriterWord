using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Enumerations;
using PhonoWriterWord.Managers;
using PhonoWriterWord.Predictions.Providers;
using PhonoWriterWord.Services.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhonoWriterWord.Predictions
{
    public class PredictionsProvidersManager
    {
        //IConfiguration _configuration;
        //LanguagesManager _languagesManager;
        string _lastInput;
        Log _log;
        Dictionary<string, PredictionsProvider> _providers;

        public event EventHandler<PredictionsFoundArgs> PredictionsFound;

        public Dictionary<string, PredictionsProvider> Providers
        {
            get { return _providers; }
        }

        public PredictionsProvidersManager()
        {
            //_log = new Log(App.Current.LogService, GetType().Name);
            _providers = new Dictionary<string, PredictionsProvider>();

            //_configuration = App.Current.Configuration;
            //_languagesManager = App.Current.LanguagessManager;
        }

        public void AddProvider(string identifier, PredictionsProvider provider)
        {
            _providers.Add(identifier, provider);
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
        /// 3) Sort them by balancing (except 2.3).
        /// 4) Join 2.1 and 2.2 (lists with existing words).
        /// 5) Remove pictureless existing words if necessary.
        /// 6) Join the three lists (and ignore 2.3 if 5 is applied
        ///    -> non existing words = pictureless !).
        /// 7) Compare input's hash to predictions hash and put the most
        ///    similar word on top.
        /// 
        /// </summary>
        /// <returns>Filtered results</returns>
        /// <param name="predictions">Predictions</param>
        /// <param name="input">Input</param>
        public List<string> Filter(List<string> predictions, string input)
        {
            //var language = LanguagesManager.CurrentLanguage;
            var language = new Language(1, "fr");
            List<Word> words = language.Words;

            // Regroup by occurencies (high = > 1 occurences, low = 1 occurency).
            List<string> highs = predictions.Where(p => predictions.Count(c => c == p) > 1).GroupBy(s => s).OrderByDescending(g => predictions.Count(p => p == g.Key)).Select(g => g.Key).ToList();
            List<string> lows = predictions.Where(p => predictions.Count(c => c == p) == 1).GroupBy(s => s).Select(g => g.Key).ToList();
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
            predictions = sortedHigh.Select(s => s.Text).ToList();

            // Add inexistants if necessary.
            //if (!_configuration.PictographicHidePictureless)
            predictions.AddRange(inexistants);

            // TODO : fix this
            // Prevent overflow of predictions (rare bug) until it's fixed.
            int max = 7; //_configuration.ClassicPredictionsNumber + _configuration.PhoneticPredictionNumber + _configuration.FuzzyPredictionNumber;
            if (predictions.Count > max)
                predictions.RemoveRange(max, predictions.Count - max);

            //// Put the most similar hashed word on top.
            //var inputHash = Algorithms.Fuzzy.Fuzzy.Current(input, (LanguagesEnum)_configuration.Dictionary);
            //for (int i = 0; i < predictions.Count; i++)
            //{
            //    string prediction = predictions[i];
            //    var predictionHash = Algorithms.Fuzzy.Fuzzy.Current(predictions[i], (LanguagesEnum)_configuration.Dictionary);
            //    if (inputHash == predictionHash)
            //    {
            //        predictions.RemoveAt(i);
            //        predictions.Insert(0, prediction); // Put word on top.
            //        break;
            //    }
            //}

            return predictions;
        }

        public void RemoveProvider(string identifier)
        {
            _providers.Remove(identifier);
        }

        public void Request(string previousWord, string context, string input, LanguagesEnum language, int classicPredictions, int fuzzyPredictions, int phoneticPredictions, int relationshipPredictions)
        {
            Request(_providers.Values.ToList(), previousWord, context, input, language, classicPredictions, fuzzyPredictions, phoneticPredictions, relationshipPredictions);
        }

        public void Request(List<PredictionsProvider> providers, string previousWord, string context, string input, LanguagesEnum language, int classicPredictions, int fuzzyPredictions, int phoneticPredictions, int relationshipPredictions)
        {
            // Prevent empty inputs.
            if (string.IsNullOrWhiteSpace(previousWord) && string.IsNullOrWhiteSpace(input))
            {
                _lastInput = string.Empty;

                PredictionsFound?.Invoke(this, new PredictionsFoundArgs(input, new List<string>()));
                return;
            }

            _lastInput = input;

            Task.Run(() =>
            {
                _log.Debug("Requested '{0}'", input);

                List<string> predictions = new List<string>();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    // Sleep for a while to prevent computing too soon (less collisions).
                    int time = 20;
                    if (phoneticPredictions > 0) time += 100;
                    if (fuzzyPredictions > 0) time += 50;
                    Thread.Sleep(time);

                    // Soft cancel (stop request).
                    if (input != _lastInput)
                        return;

                    foreach (var provider in providers)
                        predictions.AddRange(
                            provider.Request(
                                input,
                                context,
                                language,
                                classicPredictions,
                                fuzzyPredictions,
                                phoneticPredictions,
                                0
                            ).Predictions
                        );

                    // Soft cancel (stop request).
                    if (input != _lastInput)
                        return;

                    predictions = Filter(predictions, input.Trim());
                }

                if (!string.IsNullOrWhiteSpace(previousWord) && input.Length == 0)
                {
                    foreach (var provider in providers)
                    {
                        predictions.InsertRange(
                            0,
                            provider.Request(
                                input,
                                context,
                                language,
                                0,
                                0,
                                0,
                                150
                            ).Predictions //.Where(w => w.StartsWith(input, StringComparison.CurrentCultureIgnoreCase)).Reverse().Take(5)
                        );
                        predictions = predictions.Distinct().ToList();
                    }
                }

                // TODO: Eventually find a way to make this plateform dependent. The following line where used before mac native next word prediction
                //if (!string.IsNullOrWhiteSpace(previousWord))
                //{
                //    foreach (var provider in providers)
                //    {
                //        predictions.InsertRange(
                //            0,
                //            provider.Request(
                //                previousWord,
                //                context,
                //                language,
                //                0,
                //                0,
                //                0,
                //                150
                //            ).Predictions //.Where(w => w.StartsWith(input, StringComparison.CurrentCultureIgnoreCase)).Reverse().Take(5)
                //        );
                //        predictions = predictions.Distinct().ToList();
                //    }
                //}
                PredictionsFound?.Invoke(this, new PredictionsFoundArgs(input, predictions));
            });
        }
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
        List<string> _predictions;

        public string Input
        {
            get { return _input; }
        }

        public List<string> Predictions
        {
            get { return _predictions; }
        }

        public PredictionsFoundArgs(string input, List<string> predictions)
        {
            _input = input;
            _predictions = predictions;
        }
    }
}
