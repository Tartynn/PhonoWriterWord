using System.Text;
using System.Text.RegularExpressions;
namespace PhonoWriterWord.Algorithms.Fuzzy.SoundEx
{
    /// -------------------------------------------------------------------------
    /// <summary>
    /// Soundex v1.
    /// </summary>
    /// -------------------------------------------------------------------------
    public class Soundex : SoundexBase
    {
        private readonly string _NOTACCEPTEDLETTERS = string.Empty;

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Create the soundex object.
        /// </summary>
        /// <param name="word"> The word to code. </param>
        /// -------------------------------------------------------------------------
        public Soundex(string word) : base(word)
        {
            _NOTACCEPTEDLETTERS = string.Format("{0}|{1}|{2}", _VOWELS, _SPECVOWEL, _DUMB);
        }

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Calculate the soundex code (v1).
        /// </summary>
        /// -------------------------------------------------------------------------
        public override string GetSoundExCode()
        {
            int len = 5;

            // Stock the soundex code
            StringBuilder sb = new StringBuilder(len, len);
            // Add the first character
            sb.Append(base.GetNotAccent(base._word[0]));

            if (base._word.Length > 1)
            {
                int code = 0;
                int prevCode = 0;
                // Remove the vowels and the dumb letters
                base._word = Regex.Replace(base._word.Substring(1), _NOTACCEPTEDLETTERS, string.Empty);
                for (int i = 0; i < base._word.Length; i++)
                {
                    // Transform the current character to it's associated value
                    switch (base._word[i])
                    {
                        /* Bilabiales */
                        case 'B':
                        case 'P': code = 1; break;
                        /* Labiodentales */
                        case 'C':
                        case 'Ç':
                        case 'K':
                        case 'Q': code = 2; break;
                        /* Dentales */
                        case 'D':
                        case 'T': code = 3; break;
                        /* Alvéolaires */
                        case 'L': code = 4; break;
                        /* Vélaires */
                        case 'M':
                        case 'N': code = 5; break;
                        /* Laryngales */
                        case 'R': code = 6; break;
                        /* Labiodentales */
                        case 'G':
                        case 'J': code = 7; break;
                        /* Labiodentales */
                        case 'S':
                        case 'X':
                        case 'Z': code = 8; break;
                        /* Bilabiales */
                        case 'F':
                        case 'V': code = 9; break;
                    }
                    // Cannot add two times the same code !
                    if (code != prevCode) sb.Append(code);
                    if (sb.Length == len) break; // The soundex code has a lenght of 4 characters
                    prevCode = code;
                }
            }
            // Pad Left with '0' if needed
            sb.Append('0', len - sb.Length);
            return sb.ToString();
        }
    }
}