using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Database.Models
{
    public class Pair
    {
        private int id;
        private int currentWord;
        private int nextWord;
        private int occurrence;
        private int isUpdated;

        public Pair()
        {
            this.occurrence = 1;
            this.isUpdated = 0;
        }

        public Pair(int currentWord, int nextWord) : this()
        {
            this.currentWord = currentWord;
            this.nextWord = nextWord;
        }

        public Pair(Word currentWord, Word nextWord) : this()
        {
            this.currentWord = currentWord.Id;
            this.nextWord = nextWord.Id;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int CurrentWord
        {
            get { return currentWord; }
            set { currentWord = value; }
        }

        public int NextWord
        {
            get { return nextWord; }
            set { nextWord = value; }
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

        public Word GetCurrentWord()
        {
            return DatabaseController.databaseController.WordsController.Research(currentWord);
        }

        public Word GetNextWord()
        {
            return DatabaseController.databaseController.WordsController.Research(nextWord);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Word [ Number: {0}, CurrentWord : {1}, NextWord : {2}, Occurrence : {3}, IsUpdated : {4} ]", id, currentWord, nextWord, occurrence, isUpdated);
            return sb.ToString();
        }
    }
}
