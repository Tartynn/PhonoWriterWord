using System.Data.SQLite;
using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Database.Controllers
{
    public class WordsController : AbstractController
    {
        public static string createQuery = "INSERT INTO word (language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated) VALUES (:languageId, :definitionId, :imageId, :text, :occurrence, :fuzzyHash, :phonetic, :isUpdated)";
        public static string researchQuery = "SELECT id, language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated FROM word WHERE id=:id";
        public static string researchByTextQuery = "SELECT id, language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated FROM word WHERE text=:text AND language_id=:languageId";
        public static string researchByBeginningQuery = "SELECT id, language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated FROM word WHERE text LIKE :text AND language_id=:languageId AND LENGTH(text)>=:minCaracteres ORDER BY occurrence DESC";
        public static string researchByBeginningWithLimitQuery = researchByBeginningQuery + " LIMIT :limite";
        public static string researchAllWordsQuery = "SELECT id, language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated FROM word WHERE language_id=:languageId";
        public static string researchByPairFWQuery = "SELECT id, language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated FROM word WHERE id= ( SELECT currentWord FROM pair WHERE (id = :pairId))";
        public static string researchByPairLWQuery = "SELECT id, language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated FROM word WHERE id= ( SELECT nextWord FROM pair WHERE (id = :pairId))";
        public static string researchByImageQuery = "SELECT id, language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated FROM word WHERE image_id=:imageId";
        public static string researchByDefinitionQuery = "SELECT id, language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated FROM word WHERE definition_id=:definitionId";
        public static string researchByLanguageQuery = "SELECT id, language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated FROM word WHERE language_id=:languageId";
        public static string researchByHashQuery = "SELECT id, language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated FROM word WHERE fuzzy_hash=:fuzzyHash";
        public static string updateQuery = "UPDATE word SET definition_id=:definitionId, image_id=:imageId, text=:text, occurrence=:occurrence, fuzzy_hash=:fuzzyHash, phonetic=:phonetic, is_updated=:isUpdated WHERE id=:id";
        public static string deleteQuery = "DELETE FROM word WHERE id=:id";
        public static string purgeWordsQuery = "DELETE FROM word WHERE is_updated=1";

        public WordsController(DatabaseController controller) : base(controller)
        {
        }

        public int Create(Word word)
        {
            return (int)DatabaseController.DoTransaction((command, transaction) =>
            {
                SQLiteParameter paramLanguage = new SQLiteParameter("languageId", word.Language);
                SQLiteParameter paramDefinition = new SQLiteParameter("definitionId", DBNull.Value);
                SQLiteParameter paramImage = new SQLiteParameter("imageId", DBNull.Value);
                SQLiteParameter paramText = new SQLiteParameter("text", word.Text);
                SQLiteParameter paramOccurrence = new SQLiteParameter("occurrence", word.Occurrence);
                SQLiteParameter paramFuzzyHash = new SQLiteParameter("fuzzyHash", word.FuzzyHash);
                SQLiteParameter paramPhonetic = new SQLiteParameter("phonetic", word.Phonetic);
                SQLiteParameter paramIsUpdated = new SQLiteParameter("isUpdated", Convert.ToInt32(word.IsUpdated));

                //paramLanguage.Value = word.Language;
                //paramDefinition.Value = DBNull.Value;
                //paramImage.Value = DBNull.Value;
                //paramText.Value = word.Text;
                //paramOccurrence.Value = word.Occurrence;
                //paramFuzzyHash.Value = word.FuzzyHash;
                //paramPhonetic.Value = word.Phonetic;
                //paramIsUpdated.Value = Convert.ToInt32(word.IsUpdated);

                if (word.Definition > 0) paramDefinition.Value = word.Definition;
                if (word.Image > 0) paramImage.Value = word.Image;

                command.CommandText = createQuery;
                command.Parameters.Add(paramLanguage);
                command.Parameters.Add(paramDefinition);
                command.Parameters.Add(paramImage);
                command.Parameters.Add(paramText);
                command.Parameters.Add(paramOccurrence);
                command.Parameters.Add(paramFuzzyHash);
                command.Parameters.Add(paramPhonetic);
                command.Parameters.Add(paramIsUpdated);

                string query = command.CommandText;

                foreach (SQLiteParameter p in command.Parameters)
                    query = query.Replace(p.ParameterName, p.Value.ToString());

                //Execute query
                command.ExecuteNonQuery();

                // Return the new ID.
                command.CommandText = "SELECT last_insert_rowid()";
                int.TryParse(command.ExecuteScalar().ToString(), out int id);

                return id;
            });
        }

        public void Delete(Word word)
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                SQLiteParameter param = new SQLiteParameter("id", word.Id);

                //param.Value = word.Id;

                command.CommandText = deleteQuery;
                command.Parameters.Add(param);

                command.ExecuteNonQuery();
            });
        }

        /// <summary>
        /// Parse the DataRow used in DB transactions and create a new word from it.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static Word ParseRow(DataRow row)
        {
            Word word = new Word();
            word.Id = Convert.ToInt32(row.ItemArray[0]);
            word.Language = Convert.ToInt32(row.ItemArray[1]);
            if (row.ItemArray[2] != DBNull.Value) word.Definition = Convert.ToInt32(row.ItemArray[2]);
            if (row.ItemArray[3] != DBNull.Value) word.Image = Convert.ToInt32(row.ItemArray[3]);
            word.Text = Convert.ToString(row.ItemArray[4]);
            word.Occurrence = Convert.ToInt32(row.ItemArray[5]);
            if (row.ItemArray[6] != DBNull.Value) word.FuzzyHash = Convert.ToString(row.ItemArray[6]);
            if (row.ItemArray[7] != DBNull.Value) word.Phonetic = Convert.ToString(row.ItemArray[7]);
            word.IsUpdated = Convert.ToBoolean(row.ItemArray[8]);

            return word;
        }

        public void PurgeWords()
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                command.CommandText = purgeWordsQuery;
                command.ExecuteNonQuery();
            });
        }

        public Word Research(int id)
        {
            return (Word)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter param = new SQLiteParameter("id", id);

                //param.Value = id;
                command.CommandText = researchQuery;
                command.Parameters.Add(param);

                // Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                // Construct the object from the first row of the dataTable
                Word word = null;
                DataRow dr = dt.Rows[0];
                if (dr != null)
                {
                    word = ParseRow(dr);
                }
                else
                {
                    throw new WordNotFoundException("Word not found");
                }

                return word;
            });
        }

        public List<Word> ResearchAllWords(Language language)
        {
            return (List<Word>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramLanguage = new SQLiteParameter("languageId", language.Id);

                //paramLanguage.Value = language.Id;
                command.CommandText = researchAllWordsQuery;
                command.Parameters.Add(paramLanguage);

                //Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                //Construct the objects from the DataTable
                List<Word> words = new List<Word>();
                Word word = null;
                System.Diagnostics.Debug.WriteLine("DATA ROWS " + dt.Rows.Count);
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    word = ParseRow(dr);
                    words.Add(word);
                }

                return words;
            });
        }

        public List<Word> ResearchByBeginning(Language language, string beginning, Int32 minCharacters)
        {
            return (List<Word>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramText = new SQLiteParameter("text", beginning + "%");
                SQLiteParameter paramLanguage = new SQLiteParameter("languageId", language.Id);
                SQLiteParameter paramMinCaracteres = new SQLiteParameter("minCaracteres", minCharacters);

                //paramText.Value = beginning + "%";
                //paramLanguage.Value = language.Id;
                //paramMinCaracteres.Value = minCharacters;

                command.CommandText = researchByBeginningQuery;
                command.Parameters.Add(paramText);
                command.Parameters.Add(paramLanguage);
                command.Parameters.Add(paramMinCaracteres);

                //Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                //Construct the objects from the DataTable
                List<Word> words = new List<Word>();
                Word word = null;
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    word = ParseRow(dr);
                    words.Add(word);
                }

                return words;
            });
        }

        public List<Word> ResearchByBeginning(Language language, string beginning, Int32 nbWords, Int32 minCharacters)
        {
            return (List<Word>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramText = new SQLiteParameter("text", beginning + "%"); // Character "%" is to search all the words which begin by "beginning"
                SQLiteParameter paramLanguage = new SQLiteParameter("languageId", language.Id);
                SQLiteParameter paramLimit = new SQLiteParameter("limite", nbWords);
                SQLiteParameter paramMinCaracteres = new SQLiteParameter("minCaracteres", minCharacters);

                //paramText.Value = beginning + "%"; // Character "%" is to search all the words which begin by "beginning"
                //paramLanguage.Value = language.Id;
                //paramLimit.Value = nbWords;
                //paramMinCaracteres.Value = minCharacters;

                command.CommandText = researchByBeginningWithLimitQuery;
                command.Parameters.Add(paramText);
                command.Parameters.Add(paramLanguage);
                command.Parameters.Add(paramLimit);
                command.Parameters.Add(paramMinCaracteres);

                List<Word> words = new List<Word>();
                Word word = null;
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    word = ParseRow(dr);
                    words.Add(word);
                }

                return words;
            });
        }

        public List<Word> ResearchByDefinition(Definition definition)
        {
            return (List<Word>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramDefinition = new SQLiteParameter("definitionId", definition.Id);

                //paramDefinition.Value = definition.Id;
                command.CommandText = researchByDefinitionQuery;
                command.Parameters.Add(paramDefinition);

                DataTable dt = new DataTable();
                List<Word> words = new List<Word>();

                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    Word word = ParseRow(dr);
                    words.Add(word);
                }

                return words;
            });
        }

        public List<Word> ResearchByLanguage(Language language)
        {
            return (List<Word>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramLanguage = new SQLiteParameter("languageId", language.Id);

                //paramLanguage.Value = language.Id;

                command.CommandText = researchByLanguageQuery;
                command.Parameters.Add(paramLanguage);

                DataTable dt = new DataTable();
                List<Word> words = new List<Word>();

                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    Word word = ParseRow(dr);
                    words.Add(word);
                }

                return words;
            });
        }

        public List<Word> ResearchByImage(Image image)
        {
            return (List<Word>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramImage = new SQLiteParameter("imageId", image.Id);

                //paramImage.Value = image.Id;
                command.CommandText = researchByImageQuery;
                command.Parameters.Add(paramImage);

                List<Word> words = new List<Word>();
                Word word = null;
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    word = ParseRow(dr);
                    words.Add(word);
                }

                return words;
            });
        }

        public List<Word> ResearchByHash(string hash)
        {
            return (List<Word>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramHash = new SQLiteParameter("fuzzyHash", hash);

                //paramHash.Value = hash;
                command.CommandText = researchByHashQuery;
                command.Parameters.Add(paramHash);

                List<Word> words = new List<Word>();
                Word word = null;
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    word = ParseRow(dr);
                    words.Add(word);
                }

                return words;
            });
        }

        public Word ResearchByPairFW(Pair pair)
        {
            Word word = null;

            return (Word)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramPair = new SQLiteParameter("pairId", pair.Id);

                //paramPair.Value = pair.Id;
                command.CommandText = researchByPairFWQuery;
                command.Parameters.Add(paramPair);

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    word = ParseRow(dr);
                }

                return word;
            });
        }

        public Word ResearchByPairLW(Pair pair)
        {
            return (Word)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramPair = new SQLiteParameter("pairId", pair.Id);

                //paramPair.Value = pair.Id;
                command.CommandText = researchByPairLWQuery;
                command.Parameters.Add(paramPair);

                Word word = null;
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                //Construct the objects from the DataTable
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    word = ParseRow(dr);
                }

                return word;
            });
        }

        public Word ResearchByText(Language language, string text)
        {
            return (Word)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramText = new SQLiteParameter("text", text);
                SQLiteParameter paramLanguage = new SQLiteParameter("languageId", language.Id);

                //paramText.Value = text;
                //paramLanguage.Value = language.Id;

                command.CommandText = researchByTextQuery;
                command.Parameters.Add(paramText);
                command.Parameters.Add(paramLanguage);

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                Word word = null;
                if (dt.Rows.Count == 1)
                {
                    DataRow dr = dt.Rows[0];
                    if (dr != null)
                        word = ParseRow(dr);
                }

                return word;
            });
        }

        public void Update(Word word)
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                SQLiteParameter paramId = new SQLiteParameter("id", word.Id);
                SQLiteParameter paramDefinition = new SQLiteParameter("definitionId", DBNull.Value);
                SQLiteParameter paramImage = new SQLiteParameter("imageId", DBNull.Value);
                SQLiteParameter paramText = new SQLiteParameter("text", word.Text);
                SQLiteParameter paramOccurrence = new SQLiteParameter("occurrence", word.Occurrence);
                SQLiteParameter paramFuzzyHash = new SQLiteParameter("fuzzyHash", word.FuzzyHash);
                SQLiteParameter paramPhonetic = new SQLiteParameter("phonetic", word.Phonetic);
                SQLiteParameter paramIsUpdated = new SQLiteParameter("isUpdated", Convert.ToInt32(word.IsUpdated));

                //paramId.Value = word.Id;
                //paramDefinition.Value = DBNull.Value;
                //paramImage.Value = DBNull.Value;
                //paramText.Value = word.Text;
                //paramOccurrence.Value = word.Occurrence;
                //paramFuzzyHash.Value = word.FuzzyHash;
                //paramPhonetic.Value = word.Phonetic;
                //paramIsUpdated.Value = Convert.ToInt32(word.IsUpdated);

                if (word.Definition > 0) paramDefinition.Value = word.Definition;
                if (word.Image > 0) paramImage.Value = word.Image;

                command.CommandText = updateQuery;
                command.Parameters.Add(paramId);
                command.Parameters.Add(paramDefinition);
                command.Parameters.Add(paramImage);
                command.Parameters.Add(paramText);
                command.Parameters.Add(paramOccurrence);
                command.Parameters.Add(paramFuzzyHash);
                command.Parameters.Add(paramPhonetic);
                command.Parameters.Add(paramIsUpdated);

                //Execute query
                command.ExecuteNonQuery();
            });
        }
    }
}
