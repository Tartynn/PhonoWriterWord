
using System;
using System.Data.SQLite;
using System.Runtime.CompilerServices;

namespace PhonoWriterWord.Services
{
    public sealed class DatabaseService
    {
        private static SQLiteConnection _connection;
        private static SQLiteTransaction _transaction;

        public string DatabasePath { get; }

        public DatabaseService(string pathToDb)
        {
            DatabasePath = pathToDb;
            _connection = CreateConnection(pathToDb);
        }

        public SQLiteTransaction BeginTransaction()
        {
            _transaction = _connection.BeginTransaction();
            return _transaction;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public SQLiteConnection CreateConnection(string pathToDb)
        {
            SQLiteConnection connection = null;

            try
            {
                connection = new SQLiteConnection("Data Source =" + pathToDb);
                connection.Open();
            }
            catch (Exception e)
            {
                connection = null;
                System.Diagnostics.Debug.WriteLine("Create connection error: " + e.Message);
            }

            return connection;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public SQLiteConnection GetConnection()
        {
            if (_connection == null)
            {
                try
                {
                    _connection = new SQLiteConnection("Data Source=" + DatabasePath + ";UseUTF8Encoding=True");
                    _connection.Open();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Get connection error: " + e.Message);
                }
            }

            return _connection;
        }

    }
}

