using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace PhonoWriterWord.Database.Controllers
{
    public class AlternativesController : AbstractController
    {
        public static string createQuery = "INSERT INTO alternative (word_id, text) VALUES (:wordId, :text)";
        public static string researchQuery = "SELECT id, word_id, text FROM alternative WHERE id=:id";
        public static string researchByTextQuery = "SELECT id, word_id, text FROM alternative WHERE text=:text";
        public static string researchByBeginningQuery = "SELECT id, word_id, text FROM alternative WHERE text LIKE :text AND LENGTH(text)>=:minCaracteres";
        public static string researchByBeginningWithLimitQuery = researchByBeginningQuery + " LIMIT :limite";
        public static string researchAllAlternativesQuery = "SELECT a.id, a.word_id, a.text FROM alternative a INNER JOIN word w ON a.word_id = w.id WHERE w.language_id=:languageId";
        public static string updateQuery = "UPDATE alternative SET word_id=:wordId, text=:text WHERE id=:id";
        public static string deleteQuery = "DELETE FROM alternative WHERE id=:id";

        public AlternativesController(DatabaseController controller) : base(controller)
        {
        }

        public int Create(Alternative alternative)
        {
            return (int)DatabaseController.DoTransaction((command, transaction) =>
            {
                SQLiteParameter paramWord = new SQLiteParameter("wordId", alternative.Word); //add "alternative.Word because SQLiteParameter needs 2 arguments : column name + value (object)
                SQLiteParameter paramText = new SQLiteParameter("text", alternative.Text); //add "alternative.Word because SQLiteParameter needs 2 arguments : column name + value (object)

                //paramWord.Value = alternative.Word;
                //paramText.Value = alternative.Text;

                command.CommandText = createQuery;
                command.Parameters.Add(paramWord);
                command.Parameters.Add(paramText);

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

        public void Delete(Alternative alternative)
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                SQLiteParameter param = new SQLiteParameter("id", alternative.Id); //add "alternative.Word because SQLiteParameter needs 2 arguments : column name + value (object)

                //param.Value = alternative.Id;

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
        private static Alternative ParseRow(DataRow row)
        {
            Alternative alternative = new Alternative();
            alternative.Id = Convert.ToInt32(row.ItemArray[0]);
            alternative.Word = Convert.ToInt32(row.ItemArray[1]);
            alternative.Text = Convert.ToString(row.ItemArray[2]);

            return alternative;
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
                Alternative alternative = null;
                DataRow dr = dt.Rows[0];
                if (dr != null)
                {
                    alternative = ParseRow(dr);
                }
                else
                {
                    throw new AlternativeNotFoundException("Alternative not found");
                }

                return alternative;
            });
        }

        public List<Alternative> ResearchAllAlternatives(Language language)
        {
            return (List<Alternative>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramLanguage = new SQLiteParameter("languageId", language.Id);

                //paramLanguage.Value = language.Id;
                command.CommandText = researchAllAlternativesQuery;
                command.Parameters.Add(paramLanguage);

                //Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                //Construct the objects from the DataTable
                List<Alternative> alternatives = new List<Alternative>();
                Alternative alternative = null;
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    alternative = ParseRow(dr);
                    alternatives.Add(alternative);
                }

                return alternatives;
            });
        }

        public List<Word> ResearchByBeginning(string beginning, Int32 minCharacters)
        {
            return (List<Word>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramText = new SQLiteParameter("text", beginning + "%");
                SQLiteParameter paramMinCaracteres = new SQLiteParameter("minCaracteres", minCharacters);

                //paramText.Value = beginning + "%";
                //paramMinCaracteres.Value = minCharacters;

                command.CommandText = researchByBeginningQuery;
                command.Parameters.Add(paramText);
                command.Parameters.Add(paramMinCaracteres);

                //Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                //Construct the objects from the DataTable
                List<Alternative> alternatives = new List<Alternative>();
                Alternative alternative = null;
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    alternative = ParseRow(dr);
                    alternatives.Add(alternative);
                }

                return alternatives;
            });
        }

        public List<Word> ResearchByBeginning(string beginning, Int32 nbWords, Int32 minCharacters)
        {
            return (List<Word>)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramText = new SQLiteParameter("text", beginning + "%"); // Character "%" is to search all the words which begin by "beginning"
                SQLiteParameter paramLimit = new SQLiteParameter("limite", nbWords);
                SQLiteParameter paramMinCaracteres = new SQLiteParameter("minCaracteres", minCharacters);

                //paramText.Value = beginning + "%"; // Character "%" is to search all the words which begin by "beginning"
                //paramLimit.Value = nbWords;
                //paramMinCaracteres.Value = minCharacters;

                command.CommandText = researchByBeginningWithLimitQuery;
                command.Parameters.Add(paramText);
                command.Parameters.Add(paramLimit);
                command.Parameters.Add(paramMinCaracteres);

                List<Alternative> alternatives = new List<Alternative>();
                Alternative alternative = null;
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    alternative = ParseRow(dr);
                    alternatives.Add(alternative);
                }

                return alternatives;
            });
        }


        public Alternative ResearchByText(string text)
        {
            return (Alternative)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter paramText = new SQLiteParameter("text", text);

                //paramText.Value = text;

                command.CommandText = researchByTextQuery;
                command.Parameters.Add(paramText);

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                //Construct the objects from the DataTable
                List<Alternative> alternatives = new List<Alternative>();
                Alternative alternative = null;
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    alternative = ParseRow(dr);
                    alternatives.Add(alternative);
                }

                return alternatives;
            });
        }

        public void Update(Alternative alternative)
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                SQLiteParameter paramId = new SQLiteParameter("id", alternative.Id);
                SQLiteParameter paramWord = new SQLiteParameter("wordId", DBNull.Value);
                SQLiteParameter paramText = new SQLiteParameter("text", alternative.Text);

                //paramId.Value = alternative.Id;
                //paramWord.Value = DBNull.Value;
                //paramText.Value = alternative.Text;

                if (alternative.Word > 0) paramWord.Value = alternative.Word;

                command.CommandText = updateQuery;
                command.Parameters.Add(paramId);
                command.Parameters.Add(paramWord);
                command.Parameters.Add(paramText);

                //Execute query
                command.ExecuteNonQuery();
            });
        }
    }
}
