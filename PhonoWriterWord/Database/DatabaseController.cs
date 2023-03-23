using PhonoWriterWord.Database.Controllers;
using PhonoWriterWord.Services;
using PhonoWriterWord.Values;
using System;
using System.Data.SQLite;

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

        public SQLiteConnection CreateConnection() => _databaseService.CreateConnection(Constants.DATABASE_FILE);
        public SQLiteConnection GetConnection() => _databaseService.GetConnection();

        public object DoCommand(Func<SQLiteCommand, object> action)
        {
            // TODO : Try to find why GetConnection() doesn't work here.
            using (var connection = CreateConnection())
            {
                //using (var command = new SQLiteCommand(connection)) // ANCIEN
                using (var command = new SQLiteCommand { Connection = connection }) //tentative de remplacement...
                {
                    return action(command);
                }
            }
        }

        public object DoTransaction(Func<SQLiteCommand, SQLiteTransaction, object> action)
        {
            object results = null;

            var connection = GetConnection();
            {
                using (var transaction = connection.BeginTransaction())
                {
                    //using (var command = new SQLiteCommand(connection))
                    using (var command = new SQLiteCommand { Connection = connection })
                    {
                        results = action(command, transaction);
                    }

                    transaction.Commit();
                }
            }

            return results;
        }

        public void DoTransaction(Action<SQLiteCommand, SQLiteTransaction> action)
        {
            var connection = GetConnection();
            {
                using (var transaction = connection.BeginTransaction())
                {
                    //using (var command = new SQLiteCommand(connection))
                    using (var command = new SQLiteCommand { Connection = connection })
                    {
                        action(command, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

    }
}
