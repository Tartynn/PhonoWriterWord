using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Database.Models
{
    public class Word
    {
        private int id;
        private string text;
        private int occurrence;
        private List<Pair> pairs;
        private int language;
        private int isUpdated;
        private int image;
        private int definition;
        private string fuzzy_hash;
        private string phonetic;


        public Word()
        {
            this.pairs = new List<Pair>();
            this.text = "";
            this.occurrence = 1;
            this.isUpdated = 0;
            this.phonetic = "";
            this.fuzzy_hash = "";
            this.phonetic = "";
        }

        public Word(int language) : this()
        {
            this.language = language;
        }

        public Word(int language, string word, string fuzzyHash) : this(language)
        {
            Text = word;
            FuzzyHash = fuzzyHash;
        }

        public Word(int language, string word, string fuzzyHash, int definition) : this(language, word, fuzzyHash)
        {
            this.definition = definition;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int Image
        {
            get { return image; }
            set { image = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public int Occurrence
        {
            get { return occurrence; }
            set { occurrence = value; }
        }

        public bool IsUpdated
        {
            get { return isUpdated == 1; }
            set { isUpdated = value ? 1 : 0; }
        }

        public int Language
        {
            get { return language; }
            set { language = value; }
        }

        public int Definition
        {
            get { return definition; }
            set { definition = value; }
        }

        public string FuzzyHash
        {
            get { return fuzzy_hash; }
            set { fuzzy_hash = value; }
        }
        public string Phonetic
        {
            get { return phonetic; }
            set { phonetic = value; }
        }

        public Definition GetDefinition()
        {
            return DatabaseController.databaseController.DefinitionsController.Research(definition);
        }

        public Image GetImage()
        {
            return DatabaseController.databaseController.ImagesController.Research(image);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Word [ Id: {0}, Text : {1}, Occurrence : {2}, Language : {3}, Definition : {4}, Image : {5}, IsUpdated : {6}, FuzzyHash : {7} ]", this.Id, this.Text, this.Occurrence, this.Language, this.Definition, this.Image, this.IsUpdated, this.FuzzyHash);
            return sb.ToString();
        }
    }
}
