using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PhonoWriterWord.Services.Log;
using PhonoWriterWord.Values;

namespace PhonoWriterWord.Services
{
    public sealed class DatabaseService
    {
        #region Fields

        private static SqliteConnection _connection;
        private static SqliteTransaction _transaction;


        private readonly List<System.Data.ConnectionState> badStates = new List<System.Data.ConnectionState>()
        {
            System.Data.ConnectionState.Broken,
            System.Data.ConnectionState.Closed,
            System.Data.ConnectionState.Executing,
            System.Data.ConnectionState.Fetching
        };

        #endregion

        #region Properties

        public string DatabasePath { get; }
        private readonly LogService _logService = null;

        #endregion

        #region Constructor

        public DatabaseService(string pathToDatabase, LogService logService = null)
        {
            DatabasePath = pathToDatabase;
            _logService = logService;
            _connection = CreateConnection(pathToDatabase);
        }

        #endregion

        #region Methods

        public SqliteTransaction BeginTransaction()
        {
            _transaction = _connection.BeginTransaction();
            return _transaction;
        }

        public void CloseConnection()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public SqliteConnection CreateConnection(string pathToDatabase)
        {
            SqliteConnection connection;

            try
            {
                connection = new SqliteConnection("Data Source=" + Constants.DATABASE_FILE);//pathToDatabase + ";UseUTF8Encoding=True;Version=3");
                connection.Open();
            }
            catch (Exception e)
            {
                connection = null;
                System.Diagnostics.Debug.WriteLine("connection = null - From DatabaseServices.cs");
                //_logService.Log(Log.LogModes.ERROR, "DatabaseService", e.Message);
            }

            return connection;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public SqliteConnection CreateConnection()
        {
            SqliteConnection connection = null;
            try
            {
                connection = new SqliteConnection("Data Source=" + DatabasePath + ";UseUTF8Encoding=True;Version=3");
                connection.Open();
            }
            catch (Exception e)
            {
                _logService.Log(Log.LogModes.ERROR, "DatabaseService", e.Message);
            }

            return connection;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public SqliteConnection GetConnection()
        {
            if (_connection == null)
            {
                try
                {
                    _connection = new SqliteConnection("Data Source=" + DatabasePath + ";UseUTF8Encoding=True");
                    _connection.Open();
                }
                catch (Exception e)
                {
                    _logService.Log(Log.LogModes.ERROR, "DatabaseService", e.Message);
                }
            }

            return _connection;
        }

        #endregion
    }
}

