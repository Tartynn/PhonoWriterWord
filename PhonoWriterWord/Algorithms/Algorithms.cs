using PhonoWriterWord.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Algorithms
{
    class Algorithms
    {
        public static string Current(string word, LanguagesEnum language)
        {
            return Splitter(word, language);
        }

        //public static List<string> DoubleMetaphone(string word)
        //{
        //    DoubleMetaphone dm = new DoubleMetaphone(word);
        //    return new List<string>() { dm.PrimaryKey, dm.AlternateKey };
        //}

        //public static string SoundEx(string word)
        //{
        //    return FastSoundEx.SoundEx2(word);
        //}

        public static string Splitter(string word, LanguagesEnum language)
        {
            return SplitterHashing.Hash(word, language);
        }
    }
}
