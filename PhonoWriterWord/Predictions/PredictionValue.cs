namespace PhonoWriterWord.Predictions
{
    public class PredictionValue
    {
        public PredictionValue(string prediction, float value, PredictionType type)
        {
            Prediction = prediction;
            Value = value;
            Type = type;
        }

        public string Prediction
        { get; }

        public float Value
        { get; }

        public PredictionType Type
        { get; }
    }
}
