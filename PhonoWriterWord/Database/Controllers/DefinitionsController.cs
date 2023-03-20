using Microsoft.Data.Sqlite;
using PhonoWriterWord.Database.Models;
using SQLite.Persistance.DAO.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Database.Controllers
{
    public class DefinitionsController : AbstractController
    {
        public static string createQuery = "INSERT INTO definition (text, is_updated) VALUES (:text, :isUpdated)";
        public static string researchQuery = "SELECT id, text, is_updated FROM definition WHERE id=:id";
        public static string researchByDefinitionQuery = "SELECT id, text, is_updated FROM definition WHERE text=:text";
        public static string researchByWordQuery = "SELECT d.id, d.text, d.is_updated FROM definition d JOIN mots m ON m.definition_id=d.id WHERE m.id=:wordId";
        public static string researchAllDefinitionQuery = "SELECT id, text, is_updated FROM definition";
        public static string updateQuery = "UPDATE definition SET text=:text, is_updated=:isUpdated WHERE id=:id";
        public static string deleteQuery = "DELETE FROM definition WHERE id=:id";

        public DefinitionsController(DatabaseController controller) : base(controller)
        {
        }

        public int Create(string text, int isUpdated)
        {
            // Exit if definition is null.
            if (text == null)
                return -1;

            // If definition already exists, take its id.
            Definition existing = ResearchByDefinition(text);
            if (existing != null)
                return existing.Id;

            return (int)DatabaseController.DoTransaction((command, transaction) =>
            {
                SqliteParameter paramText = new SqliteParameter("text", text);
                SqliteParameter paramIsUpdated = new SqliteParameter("isUpdated", isUpdated);

                //paramText.Value = text;
                //paramIsUpdated.Value = isUpdated;

                command.CommandText = DefinitionsController.createQuery;
                command.Parameters.Add(paramText);
                command.Parameters.Add(paramIsUpdated);

                // Execute the query
                command.ExecuteNonQuery();

                // Return the new ID.
                command.CommandText = "SELECT last_insert_rowid()";
                int.TryParse(command.ExecuteScalar().ToString(), out int id);

                return id;
            });
        }

        public void Delete(int id)
        {
            // Bypass if null.
            if (id < 1)
                return;

            DatabaseController.DoTransaction((command, transaction) =>
            {
                SqliteParameter param = new SqliteParameter("id", id);

                //param.Value = id;

                command.CommandText = DefinitionsController.deleteQuery;
                command.Parameters.Add(param);

                //Execute query
                command.ExecuteNonQuery();
            });
        }

        public Definition Research(int id)
        {
            return (Definition)DatabaseController.DoTransaction((command, transaction) =>
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
                if (dt.Rows.Count == 0) // Exit if no rows found.
                    return null;

                Definition definition = null;
                DataRow dr = dt.Rows[0];
                if (dr != null)
                {
                    definition = new Definition();
                    definition.Id = Convert.ToInt32(dr.ItemArray[0]);
                    definition.Text = Convert.ToString(dr.ItemArray[1]);
                    definition.IsUpdated = Convert.ToBoolean(dr.ItemArray[2]);
                }
                else
                {
                    throw new DefinitionNotFoundException("Definition not found");
                }

                return definition;
            });
        }

        public Definition ResearchByDefinition(string text)
        {
            return (Definition)DatabaseController.DoCommand((command) =>
            {
                SqliteParameter paramText = new SqliteParameter("text", text);

                //paramText.Value = text;
                command.CommandText = DefinitionsController.researchByDefinitionQuery;
                command.Parameters.Add(paramText);

                // Create a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                Definition definition = null;

                if (dt.Rows.Count == 0)
                    return definition;

                //Construct the object from the first row of the dataTable
                DataRow dr = dt.Rows[0];
                if (dr != null)
                {
                    definition = new Definition();
                    definition.Id = Convert.ToInt32(dr.ItemArray[0]);
                    definition.Text = Convert.ToString(dr.ItemArray[1]);
                    definition.IsUpdated = Convert.ToBoolean(dr.ItemArray[2]);
                }
                else
                {
                    throw new DefinitionNotFoundException("Definition not found");
                }

                return definition;
            });
        }

        public Definition ResearchByWord(Word word)
        {
            return (Definition)DatabaseController.DoCommand((command) =>
            {
                SqliteParameter param = new SqliteParameter("wordId", word.Id);

                //param.Value = word.Id;
                command.CommandText = DefinitionsController.researchByWordQuery;
                command.Parameters.Add(param);

                //Créate a DataTable to store the result of the query
                DataTable dt = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                Definition definition = null;

                if (dt.Rows.Count == 0)
                    return definition;

                //Construct the object from the first row of the dataTable
                DataRow dr = dt.Rows[0];
                if (dr != null)
                {
                    definition = new Definition();
                    definition.Id = Convert.ToInt32(dr.ItemArray[0]);
                    definition.Text = Convert.ToString(dr.ItemArray[1]);
                    definition.IsUpdated = Convert.ToBoolean(dr.ItemArray[2]);
                }
                else
                {
                    throw new DefinitionNotFoundException("Definition not found");
                }

                return definition;
            });
        }

        public List<Definition> ResearchAllDefinition()
        {
            return (List<Definition>)DatabaseController.DoCommand((cmd) =>
            {
                Definition definition = null;

                SqliteParameter paramDefinition = new SqliteParameter("id", definition.Id);

                //paramDefinition.Value = definition.Id;
                cmd.CommandText = DefinitionsController.researchAllDefinitionQuery;
                cmd.Parameters.Add(paramDefinition);

                DataTable dt = new DataTable();
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }

                List<Definition> definitions = new List<Definition>();


                for (Int32 i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    definition = new Definition();
                    definition.Id = Convert.ToInt32(dr.ItemArray[0]);
                    definition.Text = Convert.ToString(dr.ItemArray[1]);
                    definition.IsUpdated = Convert.ToBoolean(dr.ItemArray[2]);

                    definitions.Add(definition);
                }

                return definitions;
            });
        }

        public void Update(Definition definition)
        {
            DatabaseController.DoTransaction((command, transaction) =>
            {
                SqliteParameter paramId = new SqliteParameter("id", definition.Id);
                SqliteParameter paramText = new SqliteParameter("text", definition.Text);
                SqliteParameter paramIsUpdated = new SqliteParameter("isUpdated", Convert.ToInt32(definition.IsUpdated));

                //paramId.Value = definition.Id;
                //paramText.Value = definition.Text;
                //paramIsUpdated.Value = Convert.ToInt32(definition.IsUpdated);

                command.CommandText = DefinitionsController.updateQuery;
                command.Parameters.Add(paramId);
                command.Parameters.Add(paramText);
                command.Parameters.Add(paramIsUpdated);

                //Execute query
                command.ExecuteNonQuery();
            });
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }
    }
}
