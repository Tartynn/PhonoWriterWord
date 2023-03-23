using PhonoWriterWord.Enumerations;

namespace PhonoWriterWord.Predictions.Providers
{
    public abstract class PredictionsProvider
    {
        public abstract PredictionsResponse Request(
            string input,
            string context,
            LanguagesEnum language,
            int classicPredictions,
            int fuzzyPredictions,
            int phoneticPredictions,
            int relationshipPredictions
        );
    }
}
