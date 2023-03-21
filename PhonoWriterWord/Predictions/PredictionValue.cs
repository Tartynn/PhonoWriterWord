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
        { get; set; }

        public float Value
        { get; set; }

        public PredictionType Type
        { get; set; }
    }
}
