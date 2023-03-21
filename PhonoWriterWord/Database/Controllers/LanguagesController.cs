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
    public class LanguagesController : AbstractController
    {
        public static string createQuery = "INSERT INTO language (iso) VALUES (:iso)";
        public static string researchQuery = "SELECT id, iso FROM language WHERE id=:id";
        public static string researchAllLanguagesQuery = "SELECT id, iso FROM language";
        public static string researchByCodeQuery = "SELECT id, iso FROM language WHERE iso=:iso";
        public static string researchByWordQuery = "SELECT l.id, l.iso FROM language l JOIN word w ON w.language_id=l.id WHERE w.id=:wordId";
        public static string updateQuery = "UPDATE language SET iso=:iso WHERE id=:id";
        public static string deleteQuery = "DELETE FROM language WHERE id=:id";

        public LanguagesController(DatabaseController controller) : base(controller)
        {
        }

        public void Create(Language language)
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                SQLiteParameter paramIso = new SQLiteParameter("iso", language.Iso);

                //paramIso.Value = language.Iso;

                command.CommandText = LanguagesController.createQuery;
                command.Parameters.Add(paramIso);

                command.ExecuteNonQuery();
            });
        }

        public void Delete(Language language)
        {
            DatabaseController.DoTransaction((cmd, transaction) =>
            {
                SQLiteParameter paramId = new SQLiteParameter("id", language.Id);

                //paramId.Value = language.Id;

                cmd.CommandText = LanguagesController.deleteQuery;
                cmd.Parameters.Add(paramId);

                cmd.ExecuteNonQuery();
            });
        }

        public Language Research(Int32 id)
        {
            return (Language)DatabaseController.DoTransaction((cmd, transaction) =>
            {
                SQLiteParameter param = new SQLiteParameter("id", id);

                //param.Value = id;
                cmd.CommandText = LanguagesController.researchQuery;
                cmd.Parameters.Add(param);

                //Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                Language language = null;
                DataRow dr = dt.Rows[0];
                if (dr != null)
                {
                    language = new Language(
                        Convert.ToInt32(dr.ItemArray[0]),   // Id
                        Convert.ToString(dr.ItemArray[1])   // Iso
                        );
                }
                else
                {
                    throw new LanguageNotFoundException("Language not found");
                }

                return language;
            });
        }

        public Language ResearchByCode(String iso)
        {
            return (Language)DatabaseController.DoTransaction((cmd, transaction) =>
            {
                SQLiteParameter param = new SQLiteParameter("iso", iso);

                //param.Value = iso;
                cmd.CommandText = LanguagesController.researchByCodeQuery;
                cmd.Parameters.Add(param);

                Language language = null;

                DataTable dt = new DataTable();
                try
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        reader.Close();
                    }
                }
                catch
                {
                    return language;
                }

                DataRow dr = dt.Rows[0];
                if (dr != null)
                {
                    language = new Language();
                    language.Id = Convert.ToInt32(dr.ItemArray[0]);
                    language.Iso = Convert.ToString(dr.ItemArray[1]);
                }
                else
                {
                    throw new LanguageNotFoundException("Language not found");
                }

                return language;
            });
        }

        public Language ResearchByWord(Word word)
        {
            return (Language)DatabaseController.DoCommand((command) =>
            {
                SQLiteParameter param = new SQLiteParameter("wordId", word.Id);

                //param.Value = word.Id;
                command.CommandText = LanguagesController.researchByWordQuery;
                command.Parameters.Add(param);

                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                Language language = null;
                DataRow dr = dt.Rows[0];
                if (dr != null)
                {
                    language = new Language();
                    language.Id = Convert.ToInt32(dr.ItemArray[0]);
                    language.Iso = Convert.ToString(dr.ItemArray[1]);
                }
                else
                {
                    throw new LanguageNotFoundException("Language not found");
                }

                return language;
            });
        }

        public List<Language> ResearchAllLanguages()
        {
            return (List<Language>)DatabaseController.DoTransaction((cmd, transaction) =>
            {
                cmd.CommandText = researchAllLanguagesQuery;

                //Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                List<Language> languages = new List<Language>();
                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    if (dr != null)
                    {
                        Language language = new Language(
                            Convert.ToInt32(dr.ItemArray[0]),   // Id
                            Convert.ToString(dr.ItemArray[1])  // Iso
                            );

                        languages.Add(language);
                    }
                }

                return languages;
            });
        }

        public void Update(Language language)
        {
            DatabaseController.DoTransaction((cmd, transaction) =>
            {
                SQLiteParameter paramId = new SQLiteParameter("id", language.Iso);
                SQLiteParameter paramIso = new SQLiteParameter("iso", language.Iso);

                //paramId.Value = language.Id;
                //paramIso.Value = language.Iso;

                cmd.CommandText = LanguagesController.updateQuery;
                cmd.Parameters.Add(paramId);
                cmd.Parameters.Add(paramIso);

                cmd.ExecuteNonQuery();
            });
        }
    }
}
