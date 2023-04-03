using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Database.Models
{
    public class Language
    {
        private int id;
        private string iso;
        private List<Word> words;
        private List<Alternative> alternatives;
        private Dictionary<string, string> labels;

        public HashSet<string> WordsAsString { get; } = new HashSet<string>();

        public Language()
        {
            this.labels = new Dictionary<string, string>(){
                {"fr", "Français"},
                {"en", "English"},
                {"de", "Deutsch"},
                {"it", "Italiano"},
                {"es", "Español"}
            };
        }

        public Language(int id, string iso) : this()
        {
            this.id = id;
            this.iso = iso;
            this.words = new List<Word>();
            this.alternatives = new List<Alternative>();
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Iso
        {
            get { return iso; }
            set { iso = value; }
        }

        public string Label
        {
            get
            {
                return this.labels[this.iso];
            }
        }

        public List<Word> Words
        {
            get
            {
                if (words.Count == 0)
                {
                    var dbc = DatabaseController.databaseController;
                    if (dbc != null)
                    {
                        words = dbc.WordsController.ResearchAllWords(this);
                        words.ForEach(e => WordsAsString.Add(e.Text));
                    } else
                    {
                        System.Diagnostics.Debug.WriteLine("DBC was null");
                    }

                }
                return words;
            }
        }

        public List<Alternative> Alternatives
        {
            get
            {
                if (alternatives.Count == 0)
                {
                    alternatives = DatabaseController.databaseController.AlternativesController.ResearchAllAlternatives(this);
                }
                return alternatives;
            }
        }

        public Word Create(Word word)
        {
            Word existing = Words.Find(w => w.Text == word.Text);
            if (existing == null)
            {
                word.Id = DatabaseController.databaseController.WordsController.Create(word);
                words.Add(word);
                WordsAsString.Add(word.Text);
            }
            else
            {
                word = existing;
            }
            return word;
        }

        public Alternative Create(Alternative alternative)
        {
            alternative.Id = DatabaseController.databaseController.AlternativesController.Create(alternative);
            alternatives.Add(alternative);

            return alternative;
        }

        public void Delete(Word word)
        {
            // Remove alternatives
            List<Alternative> alts = alternatives.Where(a => a.Word == word.Id).ToList();
            foreach (Alternative alternative in alts)
                DatabaseController.databaseController.AlternativesController.Delete(alternative);

            // Remove word and relationships.
            DatabaseController.databaseController.WordsController.Delete(word);
            words.Remove(word);
            WordsAsString.Remove(word.Text);

            // Remove relationships.
            List<Pair> pairs = new List<Pair>();
            pairs.AddRange(DatabaseController.databaseController.PairsController.ResearchByFirstWord(word));
            pairs.AddRange(DatabaseController.databaseController.PairsController.ResearchBySecondWord(word));
            foreach (Pair relationship in pairs)
                DatabaseController.databaseController.PairsController.Delete(relationship);

            // Remove definition if orphan.
            if (word.Definition > 0)
                if (!words.Any(c => c.Definition == word.Definition))
                    DatabaseController.databaseController.DefinitionsController.Delete(word.Definition);

            // Remove image if orphan.
            if (word.Image > 0)
            {
                Image image = word.GetImage();
                if (!words.Any(c => c.Image == word.Image) && image.IsUpdated)
                {
                    DatabaseController.databaseController.ImagesController.Delete(word.Image);
                }
            }

        }

        public void Delete(Alternative alternative)
        {
            // Remove alternative
            DatabaseController.databaseController.AlternativesController.Delete(alternative);
            alternatives.Remove(alternative);
        }

        public void Update(Word word)
        {
            DatabaseController.databaseController.WordsController.Update(word);

            // Just modify the word in the list, don't reload the whole dictionary.
            int index = words.FindIndex(w => w.Id == word.Id);
            if (index > 0)
                words[index] = word;
        }

        public void Update(Alternative alternative)
        {
            DatabaseController.databaseController.AlternativesController.Update(alternative);

            // Just modify the word in the list, don't reload the whole dictionary.
            int index = alternatives.FindIndex(a => a.Id == alternative.Id);
            if (index > 0)
                alternatives[index] = alternative;
        }
    }
}
