using System.Text;

namespace PhonoWriterWord.Database.Models
{
    public class Alternative
    {
        private int id;
        private int word;
        private string text;

        public Alternative()
        {
            this.text = "";
        }

        public Alternative(int word, string text) : this()
        {
            this.word = word;
            this.text = text;
        }

        public Alternative(Word word, string text) : this()
        {
            this.word = word.Id;
            this.text = text;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int Word
        {
            get { return word; }
            set { word = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public Word GetWord()
        {
            return DatabaseController.databaseController.WordsController.Research(word);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Alternative [ Id: {0}, Word : {1}, Text : {2} ]", id, word, text);
            return sb.ToString();
        }
    }
}
