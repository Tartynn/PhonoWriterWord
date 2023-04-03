using System.Text;
using System.Text.RegularExpressions;

namespace PhonoWriterWord.Algorithms.Fuzzy.SoundEx

{
    /// -------------------------------------------------------------------------
    /// <summary>
    /// Soundex v2.
    /// </summary>
    /// -------------------------------------------------------------------------
    public class Soundex2 : SoundexBase
    {
        private readonly string _NOTACCEPTEDLETTERS = string.Empty;

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Create the soundex object.
        /// </summary>
        /// <param name="word"> The word to code. </param>
        /// -------------------------------------------------------------------------
        public Soundex2(string word) : base(word)
        {
            _NOTACCEPTEDLETTERS = string.Format("{0}|{1}", _VOWELS, _DUMB);
        }

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Calculate the soundex code (v2).
        /// </summary>
        /// -------------------------------------------------------------------------
        public override string GetSoundExCode()
        {
            int length = 4;

            StringBuilder sb = new StringBuilder();
            // Remove the accents
            foreach (char c in base._word) sb.Append(base.GetNotAccent(c));
            // Replace...
            sb.Replace("GUI", "KI").Replace("GUE", "KE").Replace("GA", "KA").Replace("GO", "KO");
            sb.Replace("GU", "K").Replace("CA", "KA").Replace("CO", "KO").Replace("CU", "KU");
            sb.Replace("Q", "K").Replace("CC", "K").Replace("CK", "K");
            // Remove not accepted letters
            string word = sb.ToString();
            word = word[0] + Regex.Replace(word.Substring(1), _NOTACCEPTEDLETTERS, "A");
            sb = new StringBuilder(word);
            // Replace...
            sb.Replace("MAC", "MCC").Replace("ASA", "AZA").Replace("KN", "NN").Replace("PF", "FF");
            sb.Replace("SCH", "SSS").Replace("PH", "FF");
            // Replace...
            word = Regex.Replace(sb.ToString(), "([^C|^S])H", "$1");
            word = Regex.Replace(word, "([^A])Y", "$1");
            word = Regex.Replace(word, "(.*)[A|D|T|S]$", "$1");
            word = word[0] + Regex.Replace(word.Substring(1), "A", string.Empty);
            word = Regex.Replace(word, @"(\D)\1+", "$1");
            // Pad Left with empty char if needed
            return (word.Length > length ? word.Substring(0, length) : word.PadRight(length, ' '));
        }
    }
}