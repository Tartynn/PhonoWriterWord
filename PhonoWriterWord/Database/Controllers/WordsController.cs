using Microsoft.Data.Sqlite;
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
                SqliteParameter paramLanguage = new SqliteParameter("languageId", word.Language);
                SqliteParameter paramDefinition = new SqliteParameter("definitionId", DBNull.Value);
                SqliteParameter paramImage = new SqliteParameter("imageId", DBNull.Value);
                SqliteParameter paramText = new SqliteParameter("text", word.Text);
                SqliteParameter paramOccurrence = new SqliteParameter("occurrence", word.Occurrence);
                SqliteParameter paramFuzzyHash = new SqliteParameter("fuzzyHash", word.FuzzyHash);
                SqliteParameter paramPhonetic = new SqliteParameter("phonetic", word.Phonetic);
                SqliteParameter paramIsUpdated = new SqliteParameter("isUpdated", Convert.ToInt32(word.IsUpdated));

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

                foreach (SqliteParameter p in command.Parameters)
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
                SqliteParameter param = new SqliteParameter("id", word.Id);

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
                SqliteParameter param = new SqliteParameter("id", id);

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
                SqliteParameter paramLanguage = new SqliteParameter("languageId", language.Id);

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
                SqliteParameter paramText = new SqliteParameter("text", beginning + "%");
                SqliteParameter paramLanguage = new SqliteParameter("languageId", language.Id);
                SqliteParameter paramMinCaracteres = new SqliteParameter("minCaracteres", minCharacters);

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
                SqliteParameter paramText = new SqliteParameter("text", beginning + "%"); // Character "%" is to search all the words which begin by "beginning"
                SqliteParameter paramLanguage = new SqliteParameter("languageId", language.Id);
                SqliteParameter paramLimit = new SqliteParameter("limite", nbWords);
                SqliteParameter paramMinCaracteres = new SqliteParameter("minCaracteres", minCharacters);

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
                SqliteParameter paramDefinition = new SqliteParameter("definitionId", definition.Id);

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
                SqliteParameter paramLanguage = new SqliteParameter("languageId", language.Id);

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
                SqliteParameter paramImage = new SqliteParameter("imageId", image.Id);

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
                SqliteParameter paramHash = new SqliteParameter("fuzzyHash", hash);

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
                SqliteParameter paramPair = new SqliteParameter("pairId", pair.Id);

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
                SqliteParameter paramPair = new SqliteParameter("pairId", pair.Id);

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
                SqliteParameter paramText = new SqliteParameter("text", text);
                SqliteParameter paramLanguage = new SqliteParameter("languageId", language.Id);

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
                SqliteParameter paramId = new SqliteParameter("id", word.Id);
                SqliteParameter paramDefinition = new SqliteParameter("definitionId", DBNull.Value);
                SqliteParameter paramImage = new SqliteParameter("imageId", DBNull.Value);
                SqliteParameter paramText = new SqliteParameter("text", word.Text);
                SqliteParameter paramOccurrence = new SqliteParameter("occurrence", word.Occurrence);
                SqliteParameter paramFuzzyHash = new SqliteParameter("fuzzyHash", word.FuzzyHash);
                SqliteParameter paramPhonetic = new SqliteParameter("phonetic", word.Phonetic);
                SqliteParameter paramIsUpdated = new SqliteParameter("isUpdated", Convert.ToInt32(word.IsUpdated));

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
