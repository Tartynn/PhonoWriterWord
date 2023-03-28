using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PhonoWriterWord.Algorithms.Fuzzy.SoundEx
{
    /// <summary>
    /// 
    /// Optimized version of the SoundEx library.
    /// 
    /// Useless procedures and memory allocations were removed. Especially for version 2.
    /// Right paddings were also removed as we don't need them into Fuzzy.
    /// 
    /// SoundEx2 recommanded for best results.
    /// 
    /// </summary>
    class FastSoundEx
    {
        private const string _VOWELS = "a|à|â|ä|e|é|è|ê|ë|i|ï|î|o|ô|ö|u|ù|û|ü|y";
        private const string _SPECVOWEL = "y";
        private const string _DUMB = "h|w";
        private static string _NOTACCEPTEDLETTERS = string.Format("{0}|{1}", _VOWELS, _DUMB);
        private const int SOUNDEXCODELENGHT = 4;

        private static char RemoveAccent(char c)
        {
            switch (c)
            {
                case 'à':
                case 'â':
                case 'á':
                case 'ä': return 'a';
                case 'ç': return 's';
                case 'é':
                case 'è':
                case 'ê':
                case 'ë': return 'e';
                case 'ï':
                case 'î': return 'i';
                case 'ô':
                case 'ö': return 'o';
                case 'ù':
                case 'ú':
                case 'û':
                case 'ü': return 'u';
            }
            return c;
        }

        private static string RemoveAccents(string word)
        {
            string w = string.Empty;
            foreach (char c in word)
                w += RemoveAccent(c);
            return w;
        }

        /// <summary>
        /// 
        /// SoundEx v2 (improved)
        /// 
        /// This was optimized for lowercase strings as they normally already are in lowercase in the Fuzzy prediction procedure.
        /// ToLower()ing a string allocates a new string so we need to avoid that.
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string SoundEx2(string word)
        {
            word = RemoveAccents(word);
            word = Regex.Replace(word, @"[^\w]+", string.Empty, RegexOptions.Compiled);

            if (word.Length < 2)
                return word;


            StringBuilder sb = new StringBuilder(word);


            // Replace...
            sb.Replace("gui", "ki").Replace("gue", "ke").Replace("ga", "ka").Replace("go", "ko");
            sb.Replace("gu", "k").Replace("ca", "ka").Replace("co", "ko").Replace("cu", "ku");
            sb.Replace("q", "k").Replace("cc", "k").Replace("ck", "k");
            sb.Replace("ph", "f"); // CUSTOM ADDON
            //sb.Replace("ch", "j"); // CUSTOM ADDON


            // Remove not accepted letters.
            word = sb.ToString();
            word = word[0] + Regex.Replace(word.Substring(1), "e|i|o|u|y|h|w", "a", RegexOptions.Compiled);

            // Replace...
            sb = new StringBuilder(word);
            sb.Replace("mac", "mcc").Replace("asa", "aza").Replace("kn", "nn").Replace("pf", "ff");
            sb.Replace("sch", "sss");

            // Replace...
            word = Regex.Replace(sb.ToString(), "([^c|^s])h", "$1", RegexOptions.Compiled);
            word = Regex.Replace(word, "([^a])y", "$1", RegexOptions.Compiled);
            word = Regex.Replace(word, "(.*)[a|d|t|s]$", "$1", RegexOptions.Compiled);
            word = word[0] + Regex.Replace(word.Substring(1), "a", string.Empty, RegexOptions.Compiled);
            word = Regex.Replace(word, @"(\D)\1+", "$1", RegexOptions.Compiled);

            return word; // Removed right padding.
        }

        /// <summary>
        /// SoundEx v1
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string SoundEx(string word)
        {
            word = RemoveAccents(word);
            word = word[0] +
                Regex.Replace(
                    Regex.Replace(
                    Regex.Replace(
                    Regex.Replace(
                    Regex.Replace(
                    Regex.Replace(
                    Regex.Replace(word.Substring(1), "[aeiouyhw]", "", RegexOptions.Compiled),
                    "[bfpv]+", "1", RegexOptions.Compiled),
                    "[cgjkqsxz]+", "2", RegexOptions.Compiled),
                    "[dt]+", "3", RegexOptions.Compiled),
                    "[l]+", "4", RegexOptions.Compiled),
                    "[mn]+", "5", RegexOptions.Compiled),
                    "[r]+", "6", RegexOptions.Compiled)
                ;
            return word; // word.PadRight(SOUNDEXCODELENGHT, '0').Substring(0, SOUNDEXCODELENGHT);
        }

        /// <summary>
        /// SoundEx v1.1
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string SoundEx11(string word)
        {
            const string soundexAlphabet = "0123012#02245501262301#202";
            string soundexString = "";
            char lastSoundexChar = '?';
            word = RemoveAccents(word);

            foreach (var c in from ch in word
                              where ch >= 'a' &&
                                    ch <= 'z' &&
                                    soundexString.Length < 4
                              select ch)
            {
                char thisSoundexChar = soundexAlphabet[c - 'a'];

                if (soundexString.Length == 0)
                    soundexString += c;
                else if (thisSoundexChar == '#')
                    continue;
                else if (thisSoundexChar != '0' &&
                         thisSoundexChar != lastSoundexChar)
                    soundexString += thisSoundexChar;

                lastSoundexChar = thisSoundexChar;
            }

            return soundexString; // soundexString.PadRight(4, '0');
        }
    }
}

