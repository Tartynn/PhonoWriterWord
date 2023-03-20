using Microsoft.Data.Sqlite;
using PhonoWriterWord.Database.Controllers;
using PhonoWriterWord.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Database
{
    public class DatabaseController
    {
        private DatabaseService _databaseService;
        public DefinitionsController DefinitionsController { get; }
        public LanguagesController LanguagesController { get; }
        public ImagesController ImagesController { get; }
        public PairsController PairsController { get; }
        public WordsController WordsController { get; }
        public AlternativesController AlternativesController { get; }

        private static DatabaseController _databaseController = null;
        public static DatabaseController databaseController
        {
            get
            {
                return _databaseController;
            }
        }

        public DatabaseController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            DefinitionsController = new DefinitionsController(this);
            //LanguagesController = new LanguagesController(this);
            ImagesController = new ImagesController(this);
            PairsController = new PairsController(this);
            WordsController = new WordsController(this);
            AlternativesController = new AlternativesController(this);
            DatabaseController._databaseController = this;

        }

        public SqliteConnection CreateConnection() => _databaseService.CreateConnection();
        public SqliteConnection GetConnection() => _databaseService.GetConnection();

        public object DoCommand(Func<SqliteCommand, object> action)
        {
            // TODO : Try to find why GetConnection() doesn't work here.
            using (var connection = CreateConnection())
            {
                //using (var command = new SqliteCommand(connection)) // ANCIEN
                using (var command = new SqliteCommand { Connection = connection }) //tentative de remplacement...
                {
                    return action(command);
                }
            }
        }

        public object DoTransaction(Func<SqliteCommand, SqliteTransaction, object> action)
        {
            object results = null;

            var connection = GetConnection();
            {
                using (var transaction = connection.BeginTransaction())
                {
                    //using (var command = new SqliteCommand(connection))
                    using (var command = new SqliteCommand { Connection = connection })
                    {
                        results = action(command, transaction);
                    }

                    transaction.Commit();
                }
            }

            return results;
        }

        public void DoTransaction(Action<SqliteCommand, SqliteTransaction> action)
        {
            var connection = GetConnection();
            {
                using (var transaction = connection.BeginTransaction())
                {
                    //using (var command = new SqliteCommand(connection))
                    using (var command = new SqliteCommand { Connection = connection })
                    {
                        action(command, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

    }
}
