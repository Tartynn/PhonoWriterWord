using Microsoft.Data.Sqlite;
using PhonoWriterWord.Database.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Database.Controllers
{
    public class PairsController : AbstractController
    {
        public static string createQuery = "INSERT INTO pair (current_word_id, next_word_id, occurrence, is_updated) VALUES (:currentWord, :nextWord, :occurrence, :isUpdated)";
        public static string researchByFirstWordQuery = "SELECT id, current_word_id, next_word_id, occurrence, is_updated FROM pair WHERE current_word_id = :currentWord";
        public static string researchBySecondWordQuery = "SELECT id, current_word_id, next_word_id, occurrence, is_updated FROM pair WHERE next_word_id = :nextWord";
        public static string researchByWordsQuery = "SELECT id, current_word_id, next_word_id, occurrence, is_updated FROM pair WHERE current_word_id = :currentWord AND next_word_id = :nextWord";
        public static string researchByWordQuery = "SELECT id, current_word_id, next_word_id, occurrence, is_updated FROM pair WHERE current_word_id = :word ORDER BY occurrence DESC";
        public static string researchByWordQueryWithLimit = "SELECT id, current_word_id, next_word_id, occurrence, is_updated FROM pair WHERE current_word_id = :currentWord ORDER BY occurrence DESC LIMIT :nbWords";
        public static string updateQuery = "UPDATE pair SET current_word_id=:currentWord, next_word_id=:nextWord, occurrence=:occurrence, is_updated=:isUpdated WHERE id=:id";
        public static string deleteQuery = "DELETE FROM pair WHERE id=:id";
        public static string purgeRelationshipsQuery = "DELETE FROM pair WHERE is_updated=1";

        public PairsController(DatabaseController controller) : base(controller)
        {
        }

        public void Create(Pair pair)
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                SqliteParameter paramCurrentWord = new SqliteParameter("currentWord", pair.CurrentWord);
                SqliteParameter paramNextWord = new SqliteParameter("nextWord", pair.NextWord);
                SqliteParameter paramOccurrence = new SqliteParameter("occurrence", pair.Occurrence);
                SqliteParameter paramIsUpdated = new SqliteParameter("isUpdated", Convert.ToInt32(pair.IsUpdated));

                //Commented since the second argument was added to SqliteParameter's arguments
                //paramCurrentWord.Value = pair.CurrentWord;
                //paramNextWord.Value = pair.NextWord;
                //paramOccurrence.Value = pair.Occurrence;
                //paramIsUpdated.Value = Convert.ToInt32(pair.IsUpdated);

                command.CommandText = createQuery;
                command.Parameters.Add(paramCurrentWord);
                command.Parameters.Add(paramNextWord);
                command.Parameters.Add(paramOccurrence);
                command.Parameters.Add(paramIsUpdated);

                command.ExecuteNonQuery();
            });
        }

        public List<Pair> ResearchByFirstWord(Word word)
        {
            return (List<Pair>)DatabaseController.DoCommand((command) =>
            {
                SqliteParameter paramCurrentWord = new SqliteParameter("currentWord", word.Id);

                //paramCurrentWord.Value = word.Id;

                command.CommandText = researchByFirstWordQuery;
                command.Parameters.Add(paramCurrentWord);

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                List<Pair> pairs = new List<Pair>();
                Pair pair = null;
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    pair = new Pair();
                    pair.Id = Convert.ToInt32(dr.ItemArray[0]);
                    pair.CurrentWord = Convert.ToInt32(dr.ItemArray[1]);
                    pair.NextWord = Convert.ToInt32(dr.ItemArray[2]);
                    pair.Occurrence = Convert.ToInt32(dr.ItemArray[3]);
                    pair.IsUpdated = Convert.ToBoolean(dr.ItemArray[4]);

                    pairs.Add(pair);
                }

                return pairs;
            });
        }

        public List<Pair> ResearchBySecondWord(Word word)
        {
            return (List<Pair>)DatabaseController.DoCommand((command) =>
            {
                SqliteParameter paramNextWord = new SqliteParameter("nextWord", word.Id);

                //paramNextWord.Value = word.Id;

                command.CommandText = researchBySecondWordQuery;
                command.Parameters.Add(paramNextWord);

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                List<Pair> pairs = new List<Pair>();
                Pair pair = null;
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    pair = new Pair();
                    pair.Id = Convert.ToInt32(dr.ItemArray[0]);
                    pair.CurrentWord = Convert.ToInt32(dr.ItemArray[1]);
                    pair.NextWord = Convert.ToInt32(dr.ItemArray[2]);
                    pair.Occurrence = Convert.ToInt32(dr.ItemArray[3]);
                    pair.IsUpdated = Convert.ToBoolean(dr.ItemArray[4]);

                    pairs.Add(pair);
                }

                return pairs;
            });
        }

        public List<Pair> ResearchByWord(Word word)
        {
            return (List<Pair>)DatabaseController.DoCommand((command) =>
            {
                SqliteParameter paramWord = new SqliteParameter("word", word.Id);

                //paramWord.Value = word.Id;

                command.CommandText = PairsController.researchByWordQuery;
                command.Parameters.Add(paramWord);

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                List<Pair> pairs = new List<Pair>();
                Pair pair = null;
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    pair = new Pair();
                    pair.Id = Convert.ToInt32(dr.ItemArray[0]);
                    pair.CurrentWord = Convert.ToInt32(dr.ItemArray[1]);
                    pair.NextWord = Convert.ToInt32(dr.ItemArray[2]);
                    pair.Occurrence = Convert.ToInt32(dr.ItemArray[3]);
                    pair.IsUpdated = Convert.ToBoolean(dr.ItemArray[4]);

                    pairs.Add(pair);
                }

                return pairs;
            });
        }

        public Pair ResearchByWords(Word currentWord, Word nextWord)
        {
            return (Pair)DatabaseController.DoCommand((command) =>
            {
                SqliteParameter paramCurrentWord = new SqliteParameter("currentWord", currentWord.Id);
                SqliteParameter paramNextWord = new SqliteParameter("nextWord", nextWord.Id);

                //paramCurrentWord.Value = currentWord.Id;
                //paramNextWord.Value = nextWord.Id;

                command.CommandText = researchByWordsQuery;
                command.Parameters.Add(paramCurrentWord);
                command.Parameters.Add(paramNextWord);

                //Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                Pair pair = null;
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    pair = new Pair();
                    pair.Id = Convert.ToInt32(dr.ItemArray[0]);
                    pair.CurrentWord = Convert.ToInt32(dr.ItemArray[1]);
                    pair.NextWord = Convert.ToInt32(dr.ItemArray[2]);
                    pair.Occurrence = Convert.ToInt32(dr.ItemArray[3]);
                    pair.IsUpdated = Convert.ToBoolean(dr.ItemArray[4]);
                }

                return pair;
            });
        }

        public List<Pair> ResearchByWordWithLimit(Word word, Int32 nbWords)
        {
            return (List<Pair>)DatabaseController.DoCommand((command) =>
            {
                SqliteParameter paramCurrentWord = new SqliteParameter("currentWord", word.Id);
                SqliteParameter paramNbWords = new SqliteParameter("nbWords", nbWords);

                //paramCurrentWord.Value = word.Id;
                //paramNbWords.Value = nbWords;

                command.CommandText = PairsController.researchByWordQueryWithLimit;
                command.Parameters.Add(paramCurrentWord);
                command.Parameters.Add(paramNbWords);

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                List<Pair> pairs = new List<Pair>();
                Pair pair = null;
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    pair = new Pair();
                    pair.Id = Convert.ToInt32(dr.ItemArray[0]);
                    pair.CurrentWord = Convert.ToInt32(dr.ItemArray[1]);
                    pair.NextWord = Convert.ToInt32(dr.ItemArray[2]);
                    pair.Occurrence = Convert.ToInt32(dr.ItemArray[3]);
                    pair.IsUpdated = Convert.ToBoolean(dr.ItemArray[4]);

                    pairs.Add(pair);
                }

                return pairs;
            });
        }

        public void Update(Pair pair)
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                SqliteParameter paramId = new SqliteParameter("id", pair.Id);
                SqliteParameter paramCurrentWord = new SqliteParameter("currentWord", pair.CurrentWord);
                SqliteParameter paramNextWord = new SqliteParameter("nextWord", pair.NextWord);
                SqliteParameter paramOccurrence = new SqliteParameter("occurrence", pair.Occurrence);
                SqliteParameter paramIsUpdated = new SqliteParameter("isUpdated", Convert.ToInt32(pair.IsUpdated));

                //paramId.Value = pair.Id;
                //paramCurrentWord.Value = pair.CurrentWord;
                //paramNextWord.Value = pair.NextWord;
                //paramOccurrence.Value = pair.Occurrence;
                //paramIsUpdated.Value = Convert.ToInt32(pair.IsUpdated);

                command.CommandText = updateQuery;
                command.Parameters.Add(paramId);
                command.Parameters.Add(paramCurrentWord);
                command.Parameters.Add(paramNextWord);
                command.Parameters.Add(paramOccurrence);
                command.Parameters.Add(paramIsUpdated);

                command.ExecuteNonQuery();
            });
        }

        public void Delete(Pair pair)
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                SqliteParameter paramId = new SqliteParameter("id", pair.Id);

                //paramId.Value = pair.Id;

                command.CommandText = deleteQuery;
                command.Parameters.Add(paramId);

                command.ExecuteNonQuery();
            });
        }

        public void PurgeRelationships()
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                command.CommandText = purgeRelationshipsQuery;

                command.ExecuteNonQuery();
            });
        }
    }
}
