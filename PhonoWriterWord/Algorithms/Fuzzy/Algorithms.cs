using PhonoWriterWord.Algorithms.Fuzzy.SoundEx;
using PhonoWriterWord.Enumerations;
using System.Collections.Generic;


namespace PhonoWriterWord.Algorithms.Fuzzy
//
// FUZZY ALGORITHMS
//
// Some algorithms for the fuzzy prediction are listed here : http://jellyfish.readthedocs.io/en/latest/phonetic.html
//
// For now, SoundEx works quite good although DoubleMetaphone seems to be even better.
// The reason DoubleMetaphone would be nice to use is that we don't need to achieve a distance comparison
// between the given word's and candidates soundex no more.
// 
// For instance :
//  SoundEx for "broliam" <-> "problème" is BRBLM <-> PRPLM
//  DoubleMetaphone for "broliam" <-> "problème" is PRPL <-> PRPL
//
// So, As you can see, SoundEx still need a distance comparison to be sure that the words are similar.
// The DoubleMetaphone doesn't require that as the keys are already a lot similar.
// The only thing to do is to check out if the DoubleMetaphone doesn't compute the same key for too much words.
// Otherwise, you'll have a lot of candidates which have nothing to do with the given word and the results will
// be corrupted.
//
//
// Note : candidates = words from dictionary to compare to the given word.
//


{
    class Algorithms
    {
        public static string Current(string word, LanguagesEnum language)
        {
            return Splitter(word, language);
        }

        public static List<string> DoubleMetaphone(string word)
        {
            DoubleMetaphone dm = new DoubleMetaphone(word);
            return new List<string>() { dm.PrimaryKey, dm.AlternateKey };
        }

        public static string SoundEx(string word)
        {
            return FastSoundEx.SoundEx2(word);
        }

        public static string Splitter(string word, LanguagesEnum language)
        {
            return SplitterHashing.Hash(word, language);
        }
    }
}
