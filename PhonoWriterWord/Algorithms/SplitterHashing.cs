using PhonoWriterWord.Enumerations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PhonoWriterWord.Algorithms
{
    /// <summary>
    /// 
    /// INFORMATION
    /// 
    /// SplitterHashing is a custom hashing method for PhonoWriter which is
    /// based on the SoundEx and Metaphone methods.
    /// 
    /// First, we filter the characters with some replace/remove operations to
    /// make it "phonetic". Then, we separate the consonants and the vowels.
    /// As we usually find more consonants than vowels, we can check at first
    /// the consonants suite and then, the vowels when comparing two hashes.
    /// This also allows the user to make more mistakes like characters permutation.
    /// 
    /// 
    /// USAGE
    /// 
    /// SplitterHashing.Hash("country"); // Return : "kntr,ouy"
    /// SplitterHashing.Split("kntr,ouy"); // Return : {"kntr", "ouy"}
    /// 
    /// 
    /// AUTHOR
    /// 
    /// Cyriaque Skrapits <cyriaque.skrapits@paraplegie.ch>
    ///  
    /// </summary>
    public class SplitterHashing
    {
        static readonly char SEPARATOR = ',';

        public static string Hash(string input, LanguagesEnum language)
        {
            string normalized = Normalize(input.ToLower(), language);

            string consonants = Regex.Replace(normalized, "[^bcdfghjklmnpqrstvwxz]*", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            string vowels = Regex.Replace(normalized, "[^aeiouyàâáäç]*", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Cut strings to 9 chars max.
            if (consonants.Length > 9)
                consonants = consonants.Substring(0, 9);
            if (vowels.Length > 9)
                vowels = vowels.Substring(0, 9);

            return consonants + SEPARATOR + vowels;
        }

        public static string[] Split(string hash)
        {
            return hash.Split(SEPARATOR);
        }

        public static bool EndsWith(StringBuilder sb, string target, bool caseInsensetive = false)
        {
            // if sb length is less than target string
            // it cannot end with it
            if (sb.Length < target.Length)
                return false;

            var offset = sb.Length - target.Length;
            for (int i = sb.Length - 1; i >= offset; i--)
            {
                var left = sb[i];
                var right = target[i - offset];

                if (caseInsensetive)
                {
                    left = char.ToUpper(left, CultureInfo.InvariantCulture);
                    right = char.ToUpper(right, CultureInfo.InvariantCulture);
                }

                if (left != right)
                    return false;
            }
            return true;
        }

        private static string Normalize(string word, LanguagesEnum language)
        {
            if (word.Length < 3)
                return word;

            StringBuilder sb = new StringBuilder(word);

            if (language == LanguagesEnum.Francais)
            {
                // Deletions

                if (sb.Length > 2)
                {
                    if (EndsWith(sb, "s"))
                        sb.Length--;
                    else if (EndsWith(sb, "t"))
                        sb.Length--;
                    else if (EndsWith(sb, "x"))
                        sb.Length--;
                    else if (EndsWith(sb, "e"))
                        sb.Length--;
                }


                // Substitutions

                // B-P rule
                sb.Replace("b", "p");
                // T-D rules
                sb.Replace("d", "t");
                // C-G-K-Q rules
                sb.Replace("gui", "ki").Replace("gue", "ke").Replace("ga", "ka").Replace("go", "ko");
                sb.Replace("gn", "ni").Replace("gu", "k").Replace("gl", "kl").Replace("gr", "kr").Replace("gh", "k").Replace("gs", "ks");
                sb.Replace("ng", "nk");
                sb.Replace("ca", "ka").Replace("co", "ko").Replace("cu", "ku").Replace("cr", "kr");
                sb.Replace("qu", "k").Replace("q", "k").Replace("ck", "k").Replace("x", "ks").Replace("acc", "aks");
                // S rules
                sb.Replace("z", "s").Replace("ç", "s").Replace("ce", "se").Replace("ci", "si").Replace("cy", "si");
                // X rules
                sb.Replace("sch", "j").Replace("ch", "j").Replace("sh", "j").Replace("cc", "j").Replace("g", "j"); ;
                // F rules
                sb.Replace("ph", "f").Replace("v", "f");
                // O rules          
                sb.Replace("eau", "o");
                sb.Replace("au", "o");
                // Others
                sb.Replace("oin", "o!n").Replace("oi", "oa").Replace("o!n", "oin");
                sb.Replace("ain", "a!n").Replace("ai", "e").Replace("a!n", "ain");
                sb.Replace("oeu", "e").Replace("eu", "e");
                sb.Replace("oinn", "on").Replace("oin", "o").Replace("oi", "oe");
                sb.Replace("aine", "en").Replace("ain", "en").Replace("ain", "e").Replace("ai", "e"); ;
                sb.Replace("inin", "ene").Replace("inn", "enn").Replace("ina", "ene").Replace("ini", "ene").Replace("ino", "eno").Replace("inu", "enu").Replace("in", "e");
                sb.Replace("onn", "!nn").Replace("one", "!ne").Replace("on", "e").Replace("!", "o");
                // Silent rule
                sb.Replace("h", "");

            }
            else if (language == LanguagesEnum.Deutsch)
            {
                // See : http://www.legastheniepraxis.at/legasthenie2.html

                sb.Replace("b", "p");
                sb.Replace("d", "t");

                sb.Replace("ach", "ar").Replace("ech", "er");

                sb.Replace("tsä", "jä").Replace("tse", "je").Replace("tsi", "ji").Replace("tsö", "jö").Replace("tsü", "jü").Replace("tsy", "jy");

                sb.Replace("sche", "je").Replace("schi", "ji").Replace("che", "je").Replace("chi", "ji");
                sb.Replace("chr", "kr");
                sb.Replace("chs", "ks");
                sb.Replace("sch", "j").Replace("ch", "j").Replace("sh", "j");

                sb.Replace("ca", "ka").Replace("co", "ko").Replace("cu", "ku").Replace("qu", "k").Replace("g", "k").Replace("c", "k");

                sb.Replace("kti", "ksi");

                sb.Replace("ph", "f").Replace("v", "f");

                sb.Replace("z", "s").Replace("ts", "s").Replace("tz", "s");

                sb.Replace("y", "i");
                sb.Replace("h", "");
            }
            else if (language == LanguagesEnum.English)
            {
                // ### Vowels substitutions ###
                // ae, a, ei : a
                sb.Replace("ae", "a").Replace("ei", "a");
                // o, ou, ow, oa, oo(r) : o 
                sb.Replace("ou", "o").Replace("ow", "o").Replace("oa", "o").Replace("oor", "or");
                // oy : oi
                sb.Replace("oy", "oi");
                // i, ee, ai, ea, y : i
                sb.Replace("ee", "i").Replace("ai", "i").Replace("ea", "i").Replace("y", "i");
                // ou : au
                sb.Replace("ou", "au");
                // oul?, u(re), oo : ou
                sb.Replace("ure", "oure").Replace("oo", "ou");
                // (p)u : u
                sb.Replace("pu", "pou");
                // u, a(re), (mm)a : e 
                sb.Replace("u", "e").Replace("are", "ere").Replace("mma", "mme");
                // (p)i : ai 
                sb.Replace("pi", "pai");

                // ###Consonants substitutions ###
                // c, ck, que : k
                sb.Replace("ck", "k").Replace("que", "k");
                // ph, gh : f
                sb.Replace("ph", "f").Replace("gh", "f");
                // t, sh, ch, tsch : ch
                sb.Replace("sh", "ch").Replace("tsch", "ch");
                // s : z
                sb.Replace("s", "z");
                // j, dg : g
                sb.Replace("j", "g").Replace("dg", "g");
                // wh : w
                sb.Replace("wh", "w");
                // nn, n : n
                sb.Replace("nn", "n");
                // the : ze
                sb.Replace("the", "ze");
                // (*)th : ss
                sb.Replace("th", "s");
            }
            else if (language == LanguagesEnum.Italiano)
            {
                // For now IT same as FR
                // Substitutions
                // B-P rule
                sb.Replace("b", "p");
                // T-D rules
                sb.Replace("d", "t");
                // C-G-K-Q rules
                sb.Replace("gui", "ki").Replace("gue", "ke").Replace("ga", "ka").Replace("go", "ko");
                sb.Replace("gn", "ni").Replace("gu", "k").Replace("gl", "kl").Replace("gr", "kr").Replace("gh", "k").Replace("gs", "ks");
                sb.Replace("ng", "nk");
                sb.Replace("ca", "ka").Replace("co", "ko").Replace("cu", "ku").Replace("cr", "kr");
                sb.Replace("qu", "k").Replace("q", "k").Replace("ck", "k").Replace("x", "ks").Replace("acc", "aks");
                // S rules
                sb.Replace("z", "s").Replace("ç", "s").Replace("ce", "se").Replace("ci", "si").Replace("cy", "si");
                // X rules
                sb.Replace("sch", "j").Replace("ch", "j").Replace("sh", "j").Replace("cc", "j").Replace("g", "j"); ;
                // F rules
                sb.Replace("ph", "f").Replace("v", "f");
                // O rules          
                sb.Replace("eau", "o");
                sb.Replace("au", "o");
                // Others
                sb.Replace("oin", "o!n").Replace("oi", "oa").Replace("o!n", "oin");
                sb.Replace("ain", "a!n").Replace("ai", "e").Replace("a!n", "ain");
                sb.Replace("oeu", "e").Replace("eu", "e");
                sb.Replace("oinn", "on").Replace("oin", "o").Replace("oi", "oe");
                sb.Replace("aine", "en").Replace("ain", "en").Replace("ain", "e").Replace("ai", "e"); ;
                sb.Replace("inin", "ene").Replace("inn", "enn").Replace("ina", "ene").Replace("ini", "ene").Replace("ino", "eno").Replace("inu", "enu").Replace("in", "e");
                sb.Replace("onn", "!nn").Replace("one", "!ne").Replace("on", "e").Replace("!", "o");
                // Silent rule
                sb.Replace("h", "");
            }
            else if (language == LanguagesEnum.Spanish)
            {
                // For now ES same as FR
                // Substitutions
                // B-P rule
                sb.Replace("b", "p");
                // T-D rules
                sb.Replace("d", "t");
                // C-G-K-Q rules
                sb.Replace("gui", "ki").Replace("gue", "ke").Replace("ga", "ka").Replace("go", "ko");
                sb.Replace("gn", "ni").Replace("gu", "k").Replace("gl", "kl").Replace("gr", "kr").Replace("gh", "k").Replace("gs", "ks");
                sb.Replace("ng", "nk");
                sb.Replace("ca", "ka").Replace("co", "ko").Replace("cu", "ku").Replace("cr", "kr");
                sb.Replace("qu", "k").Replace("q", "k").Replace("ck", "k").Replace("x", "ks").Replace("acc", "aks");
                // S rules
                sb.Replace("z", "s").Replace("ç", "s").Replace("ce", "se").Replace("ci", "si").Replace("cy", "si");
                // X rules
                sb.Replace("sch", "j").Replace("ch", "j").Replace("sh", "j").Replace("cc", "j").Replace("g", "j"); ;
                // F rules
                sb.Replace("ph", "f").Replace("v", "f");
                // O rules          
                sb.Replace("eau", "o");
                sb.Replace("au", "o");
                // Others
                sb.Replace("oin", "o!n").Replace("oi", "oa").Replace("o!n", "oin");
                sb.Replace("ain", "a!n").Replace("ai", "e").Replace("a!n", "ain");
                sb.Replace("oeu", "e").Replace("eu", "e");
                sb.Replace("oinn", "on").Replace("oin", "o").Replace("oi", "oe");
                sb.Replace("aine", "en").Replace("ain", "en").Replace("ain", "e").Replace("ai", "e"); ;
                sb.Replace("inin", "ene").Replace("inn", "enn").Replace("ina", "ene").Replace("ini", "ene").Replace("ino", "eno").Replace("inu", "enu").Replace("in", "e");
                sb.Replace("onn", "!nn").Replace("one", "!ne").Replace("on", "e").Replace("!", "o");
                // Silent rule
                sb.Replace("h", "");
            }

            // Accents
            sb = RemoveAccents(sb);

            word = sb.ToString();

            // Remove duplicate chars.
            for (int i = 0; i < word.Length - 1;)
            {
                if (word[i + 1] == word[i])
                    word = word.Remove(i + 1, 1);
                else
                    i++;
            }

            return word;
        }

        private static StringBuilder RemoveAccents(StringBuilder builder)
        {
            for (int i = 0; i < builder.Length; i++)
                builder[i] = RemoveAccent(builder[i]);
            return builder;
        }

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
            StringBuilder builder = new StringBuilder();
            foreach (char c in word)
                builder.Append(RemoveAccent(c));
            return builder.ToString();
        }
    }
}
