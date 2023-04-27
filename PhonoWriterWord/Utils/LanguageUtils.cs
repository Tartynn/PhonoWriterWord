using PhonoWriterWord.Enumerations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Utils
{
    public class LanguageUtils
    {
        public static int ConvertLanguageToInt(string twoLetterISOLanguageName)
        {
            int language = 2;

            switch (twoLetterISOLanguageName)
            {
                case "fr": language = 1; break;
                case "de": language = 3; break;
                case "it": language = 4; break;
                case "es": language = 5; break;
            }

            return language;
        }

        public static string ConvertLanguageToString(int language, bool shortName)
        {
            string code = "en-US";

            switch (language)
            {
                case (int)LanguagesEnum.Francais:
                    code = "fr-FR";
                    break;
                case (int)LanguagesEnum.Deutsch:
                    code = "de-DE";
                    break;
                case (int)LanguagesEnum.Italiano:
                    code = "it-IT";
                    break;
                case (int)LanguagesEnum.Spanish:
                    code = "es-ES";
                    break;
            }

            if (shortName)
                return code.Substring(0, 2);

            return code;
        }
    }
}
