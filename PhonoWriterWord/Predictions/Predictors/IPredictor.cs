using PhonoWriterWord.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhonoWriterWord.Predictions.Predictors
{
    public interface IPredictor
    {
        List<PredictionValue> Predict(string input, string context, int numberOfPredictions, Language language, ParallelOptions paralleldOptions);
    }
}
