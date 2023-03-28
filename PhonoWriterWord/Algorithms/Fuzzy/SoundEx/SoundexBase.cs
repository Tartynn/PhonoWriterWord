using System;
using System.Text.RegularExpressions;

namespace PhonoWriterWord.Algorithms.Fuzzy.SoundEx
{
    /// -------------------------------------------------------------------------
    /// <summary>
    /// Base soundex class.
    /// </summary>
    /// -------------------------------------------------------------------------
    public abstract class SoundexBase
    {
        protected const string _VOWELS = "A|Â|À|Ä|E|Ê|È|É|Ë|I|Ï|Î|O|Ô|Ö|U|Û|Ü|Ù|Y";
        protected const string _SPECVOWEL = "Y";
        protected const string _DUMB = "H|W";

        protected string _word = string.Empty;
        private string _oriWord = string.Empty;

        public const int SOUNDEXCODELENGHT = 4;

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Abstract, cannot be directly used.
        /// </summary>
        /// <param name="word"> The word to code. </param>
        /// -------------------------------------------------------------------------
        public SoundexBase(string word)
        {
            // Check to word
            this._oriWord = word;
            if (string.IsNullOrEmpty(word)) throw new ArgumentException("'word' cannot be null nor empty");
            // Trim, Uppercase and remove space and "-"
            this._word = Regex.Replace(word.Trim().ToUpper(), @"(\s*)(-*)", string.Empty);
        }

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Get the original word.
        /// </summary>
        /// -------------------------------------------------------------------------
        public string Word
        {
            get { return this._oriWord; }
        }

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Get the soundex code for this instance.
        /// </summary>
        /// <returns> The soundex code. </returns>
        /// -------------------------------------------------------------------------
        public abstract string GetSoundExCode();

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Remove the accent.
        /// </summary>
        /// <param name="c"> The char to check. </param>
        /// <returns> The char without accent. </returns>
        /// -------------------------------------------------------------------------
        protected char GetNotAccent(char c)
        {
            switch (c)
            {
                case 'À':
                case 'Â':
                case 'Ä': return 'A';
                case 'Ç': return 'S';
                case 'È':
                case 'É':
                case 'Ê':
                case 'Ë': return 'E';
                case 'Î':
                case 'Ï': return 'I';
                case 'Ô':
                case 'Ö': return 'O';
                case 'Û':
                case 'Ù':
                case 'Ü': return 'U';
            }
            return c;
        }
    }
}

