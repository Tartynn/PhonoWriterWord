using PhonoWriterWord.Algorithms;
using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Enumerations;
using PhonoWriterWord.Managers;
using PhonoWriterWord.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhonoWriterWord.Predictions.Predictors
{
	class PredictionFuzzy : Prediction
	{
        private List<PredictionValue> _results;

        public PredictionFuzzy()
		{
			_name = "Fuzzy";
		}

		public override List<PredictionValue> Work(string input, ParallelOptions parallelOptions)
		{
			List<PredictionValue> results = new List<PredictionValue>();


			if (parallelOptions.CancellationToken.IsCancellationRequested)
				return results;


			//	if (!_app.Configuration.FuzzyPredictionActivated)
				//	return results;


			int numberOfPredictions = 9;// _app.Configuration.FuzzyPredictionNumber;
			//var fr = new Database.Models.Language(1, "fr");
			//var words = /*_app.LanguagesManager.CurrentLanguage*/fr.Words;
			var words = Globals.ThisAddIn.LanguagesManager.CurrentLanguage.Words;

            // Threading stuff
            int THREADS = Environment.ProcessorCount / 2;                                           // Let's use half of the available CPUs.
			int range = words.Count / THREADS;                                                      // Number of words analyzed by thread.
			ConcurrentDictionary<Word, int> wordsByValue = new ConcurrentDictionary<Word, int>();   // Lists of WordValues for each thread.


			// Input stuff
			//string hash = SplitterHashing.Hash(input, (LanguagesEnum)_app.LanguagesManager.CurrentLanguage.Id);   
			string hash = SplitterHashing.Hash(input, LanguagesEnum.Francais);  // Splitter hash for input.
			string[] inputHash = hash.Split(',');
			string hashConsonants = inputHash[0];
			string hashVowels = inputHash[1];

			if (hash == ",")
				return results;

			//
			// Threading
			//

			if (input.Length == 2)
			{
				var list = words.Where(w => w.Text.Length == 2).Where(f => DistanceUtil.LevenshteinDistance(f.Text, input) < 2).OrderByDescending(o => o.Occurrence).Take(numberOfPredictions).ToList();
				foreach (var l in list)
				{
					var p = new PredictionValue();
					p.Prediction = l.Text;
					p.Value = l.Occurrence;
					p.Type = PredictionTypes.FUZZY;
					results.Add(p);
				}

				return results;
			}

			// For each thread...
			Parallel.For(0, THREADS, (id) =>
			{
				int start = range * id;                                 // Words list's index where the analyze begins.
				int end = Math.Min(range * (id + 1), words.Count);      // Words list's index where the analyze ends (avoid out of range issues).

				// Analyze range of words in the list.
				for (int i = start; i < end; i++)
				{
					// Check if thread must be cancelled.
					if (parallelOptions.CancellationToken.IsCancellationRequested)
						return;

					// Bypass if word is too short.
					if (words[i].Text.Length < 3)
						continue;


					// Hash comparisons


					// Add words with similar hash and high balance at first position.
					if (words[i].FuzzyHash == hash)
					{
						wordsByValue.TryAdd(words[i], -words[i].Occurrence);
						continue;
					}

					// Bypass if current hash is too short (or wrong -> DB must be fixed).
					string[] currentHash = SplitterHashing.Split(words[i].FuzzyHash);
					if (currentHash.Length < 2) continue;
					string currentHashConsonants = currentHash[0];
					string currentHashVowels = currentHash[1];

					if (parallelOptions.CancellationToken.IsCancellationRequested)
						return;

					// Distance computing
					int distanceText = DistanceUtil.HotDogDistance(input, words[i].Text.ToLowerInvariant());
					//int difference = Math.Abs(_words[i].Text.Length - input.Length);
					int malus = 0; // Convert.ToInt32(currentHashConsonants != hashConsonants); // Malus if hashes consonants aren't similar.
					int distanceHashConsonants = DistanceUtil.HotDogDistance(hashConsonants, currentHashConsonants) + malus;
					int distanceHashVowels = DistanceUtil.HotDogDistance(hashVowels, currentHashVowels);
					int distanceHash = distanceHashConsonants + distanceHashVowels;// + Math.Abs(currentHash.Length - hash.Length);
					if (distanceHashConsonants < 4 && distanceHashVowels < 3)
						wordsByValue.TryAdd(words[i], distanceText + distanceHash);
				}
			});

			//
			// Select/ordering stuff.
			//

			var orderredPredictions = wordsByValue.OrderBy(o => o.Value).ThenBy(b => b.Value < 0).Take(numberOfPredictions * 2);

			foreach (var word in orderredPredictions)
			{
				if (results.Count(f => f.Prediction == word.Key.Text) == 0)
				{
					PredictionValue pv = new PredictionValue();
					{
						pv.Prediction = word.Key.Text;
						pv.Value = word.Value;
						pv.Type = PredictionTypes.FUZZY;
					};

					results.Add(pv);
					
					if (results.Count == numberOfPredictions)
						break;
				}
			}

			_results = results;
			
			return results;


			// FOR DEBUG PURPOSES
			//foreach (var result in _results)
			//	System.Diagnostics.Debug.WriteLine("\t{0} : {1}", result, wordsByValue.Where(w => w.Key.Text == result).Single().Value);
		}
	}
}

