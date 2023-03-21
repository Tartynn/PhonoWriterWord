using Microsoft.Data.Sqlite;
using PhonoWriterWord.Exceptions;
using PhonoWriterWord.Services.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhonoWriterWord.Services.UpdateService
{
    public class DesktopUpdateService
    {
        #region Variables

        // Fields
        private string _configurationPath;
        private string _imageCustomPath;
        private string _database_file;
        private string _database_user_file;
        private bool _mustUpdate;
        private LogService _logService;

        public string[] MigrationStrings { get; set; }
        // Constants
        private static readonly string APPLICATION = "phonowriter"; // Assembly.GetExecutingAssembly().GetName().Name.ToLower();

#if (WIN)
        private static readonly string PLATFORM = "";
        private static readonly string PACKAGE = "setup.exe";

#else
        private static readonly string PLATFORM = "mac";
        private static readonly string PACKAGE = "PhonoWriter.app.zip";
#endif

        private static readonly string URL_SETUP = string.Format("https://soft.jeanclaudegabus.ch/soft/{0}/{1}/{2}", APPLICATION, PLATFORM, PACKAGE);
        private static readonly string URL_VERSION = string.Format("https://soft.jeanclaudegabus.ch/soft/{0}/{1}/version.txt", APPLICATION, PLATFORM);


        // Events
        public delegate void DatabaseMigrationProgressChangedHandler(double progression, string message);
        public event DatabaseMigrationProgressChangedHandler DatabaseMigrationProgressChanged;

        #endregion

        #region Properties

        public Version ConfigurationVersion { get; private set; } // Version of current PhonoWriter installation / last version.
        public Version CurrentVersion { get; private set; } // Version of running instance / current version.
        public Version OnlineVersion { get; private set; }

        #endregion

        #region Constructor

        public DesktopUpdateService(LogService logService, Version currentVersion, string configurationPath, string imageCustomPath, string databaseFile, string databaseUserFile)
        {

            _logService = logService;
#if WIN
            MigrationStrings = new string[] {Resources.Migration.DatabaseMigration0, Resources.Migration.DatabaseMigration1,
                Resources.Migration.DatabaseMigration2, Resources.Migration.DatabaseMigration3,
                Resources.Migration.DatabaseMigration4, Resources.Migration.DatabaseMigration5,
                Resources.Migration.DatabaseMigration6};
#endif

            _mustUpdate = File.Exists(configurationPath);
            if (!_mustUpdate)
                return;

            _database_file = databaseFile;
            _database_user_file = databaseUserFile;
            _configurationPath = configurationPath;
            _imageCustomPath = imageCustomPath;

            // Get past version from config
            Regex re = new Regex(@"\<Version( xmlns="""")?\>(.*)\<\/Version\>", RegexOptions.IgnoreCase);
            Match m = re.Match(File.ReadAllText(_configurationPath));
            ConfigurationVersion = Version.Parse(m.Groups[m.Groups.Count - 1].Value);

            // Get current version
            CurrentVersion = currentVersion;
            OnlineVersion = Version.Parse("0.0.0.0");
        }

        #endregion

        #region Methods



        /// <summary>
        /// Check for a newer version available online.
        /// </summary>
        /// <exception cref="NoConnectionAvailableException"/>
        /// <returns>True if a newer version is available online.</returns>
        public async Task<bool> CheckForNewerVersion()
        {
            _logService.Log(Log.LogModes.INFO, "DesktopUpdateService", "Looking up for update at " + URL_VERSION);

            // Fetch web version.
            try
            {
                //ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                //ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL_VERSION);
                request.Method = "GET";
                var response = await request.GetResponseAsync();

                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                OnlineVersion = Version.Parse(reader.ReadToEnd());
                reader.Close();
                stream.Close();
                response.Close();

                _logService.Log(Log.LogModes.INFO, "DesktopUpdateService", "Online version : " + OnlineVersion);
            }
            catch (Exception e)
            {
                _logService.Log(Log.LogModes.ERROR, "DesktopUpdateService", e.Message);
                throw new NoConnectionAvailableException();
            }

            // Compare versions.
            var result = CurrentVersion.CompareTo(OnlineVersion);

            return result < 0; // If web version is superior...
        }

        public void Download()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(URL_SETUP));
            }
            catch
            {
                throw new NoConnectionAvailableException();
            }
        }

        public void PreMigration()
        {
            if (!_mustUpdate)
                return;

            // Add stuff here if necessary...

            // If new version is 2.2 and current installed version is less, drop user database
            if (CurrentVersion.Equals(Version.Parse("2.2.0.0")) && ConfigurationVersion.CompareTo(CurrentVersion) < 0)
            {
                if (File.Exists(_database_user_file))
                    File.Delete(_database_user_file);
            }

            /* If version older than... update config, images,...
            if (VersionOlderThan("5.0.0.0"))
            {
                // format C:
            }
            */
        }

        public void PostMigration()
        {

            if (!_mustUpdate)
                return;

            // Add stuff here if necessary...
        }

        /// <summary>
        /// 
        /// Merge old and new databases into the new one.
        /// 
        /// Basically, we just take the custom records from the old database and put them in the new one.
        /// We store their new IDs into some dictionaries and use these to update the custom records.
        /// Eg. : New words ID might collide with the existing custom words so the customs have to change their ID and
        /// we have to update the foreigner keys to their new IDs.
        /// 
        /// Warning : adapt ReportDatabaseMigration() if you add or substract the steps of this procedure.
        /// 
        /// Some documentation for the newbies :
        /// http://stackoverflow.com/questions/19968847/merging-two-sqlite-databases-which-both-have-junction-tables
        /// http://blog.tigrangasparian.com/2012/02/09/getting-started-with-sqlite-in-c-part-one/
        /// 
        /// TODO :  Use the same database as iOS's so we can minimize this
        ///         function.
        /// 
        /// </summary>
        public void MigrateDatabase()
        {
            //
            // Check for database files, otherwise, bypass the migration procedure.
            //

            if (!File.Exists(_database_file))
                return;

            ReportDatabaseMigration(0.0, MigrationStrings[0]);

            var newDatabase = _database_user_file + ".new"; // New database replacing user's.
            var copyDatabase = _database_user_file + ".copy"; // Copy of old database (to avoid file locking issues on Windows).

            // Work on a copy.
            if (File.Exists(newDatabase))
                File.Delete(newDatabase);

            if (File.Exists(copyDatabase))
                File.Delete(copyDatabase);

            File.Copy(_database_file, newDatabase);
            File.Copy(_database_user_file, copyDatabase);

            //
            // Migration procedure
            //

            string sql;

            Dictionary<string, int> images = new Dictionary<string, int>();
            Dictionary<string, int> definitions = new Dictionary<string, int>();
            Dictionary<int, int> words = new Dictionary<int, int>();

            using (var db = new SqliteConnection("Data Source=" + newDatabase + ";UseUTF8Encoding=True;foreign keys=True"))
            {
                //
                // 1) Load database
                //


                ReportDatabaseMigration(1.0 / 7, MigrationStrings[0]);

                // Attach old DB.
                db.Open();
                sql = "ATTACH '" + copyDatabase + "' AS OldDB";
                using (var command = new SqliteCommand(sql, db))
                {
                    command.ExecuteNonQuery();
                }


                //
                // 2) Images
                //


                ReportDatabaseMigration(2.0 / 7, MigrationStrings[1]);

                // Add images to the new DB.
                sql = "SELECT i.*, w.text FROM OldDB.image i INNER JOIN OldDB.word w ON w.image_id = i.id WHERE i.is_updated = 1;";
                using (var command = new SqliteCommand(sql, db))
                {
                    DataTable table = new DataTable();
                    using (var reader = command.ExecuteReader())
                    {
                        table.Load(reader);
                        reader.Close();
                    }

                    string insertQuery = "INSERT INTO image(file_name, is_updated) VALUES(:fileName, 1);";
                    string selectQuery = "SELECT last_insert_rowid()";

                    var paramFileName = new SqliteParameter("fileName", db);

                    var commandInsert = new SqliteCommand(insertQuery, db);
                    commandInsert.Parameters.Add(paramFileName);

                    var commandSelect = new SqliteCommand(selectQuery, db);

                    using (var transaction = db.BeginTransaction())
                    {
                        // For each definition...
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            DataRow row = table.Rows[i];

                            // Insert new image into new database.
                            paramFileName.Value = row[1];
                            commandInsert.ExecuteNonQuery();

                            // Get its new ID.
                            int newID = Convert.ToInt32(commandSelect.ExecuteScalar());
                            images.Add((string)row[3], newID);
                        }

                        transaction.Commit();
                    }

                    commandInsert.Dispose();
                    commandInsert = null;

                    commandSelect.Dispose();
                    commandSelect = null;
                }


                //
                // 3) Definitions
                //


                ReportDatabaseMigration(3.0 / 7, MigrationStrings[2]);

                // Add definitions to the new DB.
                sql = "SELECT d.*, w.text FROM OldDB.definition d INNER JOIN OldDB.word w ON w.definition_id = d.id WHERE d.is_updated = 1";
                using (var command = new SqliteCommand(sql, db))
                {
                    DataTable table = new DataTable();
                    using (var reader = command.ExecuteReader())
                    {
                        table.Load(reader);
                        reader.Close();
                    }

                    string insertQuery = "INSERT INTO definition(text, is_updated) VALUES(:definition, 1);";
                    string selectQuery = "SELECT last_insert_rowid()";

                    SqliteParameter paramDefinition = new SqliteParameter("definition", db);

                    var commandInsert = new SqliteCommand(insertQuery, db);
                    commandInsert.Parameters.Add(paramDefinition);

                    var commandSelect = new SqliteCommand(selectQuery, db);
                    commandSelect.Parameters.Add(paramDefinition);

                    using (var transaction = db.BeginTransaction())
                    {
                        // For each definition...
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            double index = i + 1;
                            DataRow row = table.Rows[i];

                            ReportDatabaseMigration(3 / 7 + (index / table.Rows.Count), MigrationStrings[2] + " (" + index.ToString() + "/" + table.Rows.Count.ToString() + ")");

                            paramDefinition.Value = row[1];
                            commandInsert.ExecuteNonQuery();

                            int newID = Convert.ToInt32(commandSelect.ExecuteScalar());
                            definitions.Add((string)row[3], newID);
                        }

                        transaction.Commit();
                    }

                    commandInsert.Dispose();
                    commandInsert = null;

                    commandSelect.Dispose();
                    commandSelect = null;
                }


                //
                // 4) Words
                //


                ReportDatabaseMigration(4.0 / 7, MigrationStrings[3]);

                // Add words to the new DB.
                sql = "SELECT * FROM OldDB.word WHERE is_updated = 1";
                using (var command = new SqliteCommand(sql, db))
                {
                    DataTable table = new DataTable();
                    using (var reader = command.ExecuteReader())
                    {
                        table.Load(reader);
                        reader.Close();
                    }


                    string insertQuery = "INSERT OR IGNORE INTO word(language_id, definition_id, image_id, text, occurrence, fuzzy_hash, phonetic, is_updated) VALUES(:languageId, :definitionId, :imageId, :text, :occurrence, :fuzzy_hash, :phonetic, 1);";
                    string selectQuery = "SELECT id FROM word WHERE text = :text and language_id = :languageId";
                    string updateQuery = "UPDATE word SET definition_id = :definitionId, image_id = :imageId, occurrence = :occurrence, fuzzy_hash = :fuzzy_hash, phonetic = :phonetic, is_updated = 1 WHERE text = :text and language_id = :languageId";

                    SqliteParameter paramLanguageId = new SqliteParameter("languageId", db);
                    SqliteParameter paramDefinitionId = new SqliteParameter("definitionId", db);
                    SqliteParameter paramImageId = new SqliteParameter("imageId", db);
                    SqliteParameter paramText = new SqliteParameter("text", db);
                    SqliteParameter paramOccurrence = new SqliteParameter("occurrence", db);
                    SqliteParameter paramFuzzyHash = new SqliteParameter("fuzzy_hash", db);
                    SqliteParameter paramPhonetic = new SqliteParameter("phonetic", db);

                    // Create INSERT to new database command.
                    var commandInsert = new SqliteCommand(insertQuery, db);

                    commandInsert.Parameters.Add(paramLanguageId);
                    commandInsert.Parameters.Add(paramDefinitionId);
                    commandInsert.Parameters.Add(paramImageId);
                    commandInsert.Parameters.Add(paramText);
                    commandInsert.Parameters.Add(paramOccurrence);
                    commandInsert.Parameters.Add(paramFuzzyHash);
                    commandInsert.Parameters.Add(paramPhonetic);

                    // Create SELECT number of the new word command.
                    var commandNumber = new SqliteCommand(selectQuery, db);

                    commandNumber.Parameters.Add(paramLanguageId);
                    commandNumber.Parameters.Add(paramText);

                    // Create UPDATE command
                    var commandUpdate = new SqliteCommand(updateQuery, db);

                    commandUpdate.Parameters.Add(paramLanguageId);
                    commandUpdate.Parameters.Add(paramDefinitionId);
                    commandUpdate.Parameters.Add(paramImageId);
                    commandUpdate.Parameters.Add(paramText);
                    commandUpdate.Parameters.Add(paramOccurrence);
                    commandUpdate.Parameters.Add(paramFuzzyHash);
                    commandUpdate.Parameters.Add(paramPhonetic);

                    using (var transaction = db.BeginTransaction())
                    {
                        // For each custom word...
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            double index = i + 1;
                            DataRow row = table.Rows[i];

                            ReportDatabaseMigration(4 / 7 + (index / table.Rows.Count), MigrationStrings[3] + " (" + index.ToString() + "/" + table.Rows.Count.ToString() + ")");

                            int fk_images = 0;
                            int fk_definitions = 0;

                            if (row[3] != DBNull.Value)
                                fk_images = Convert.ToInt32(row[3]);
                            if (row[2] != DBNull.Value)
                                fk_definitions = Convert.ToInt32(row[2]);

                            if (fk_images != 0 && images.ContainsKey((string)row[4]))
                                fk_images = images[(string)row[4]];
                            if (fk_definitions != 0 && definitions.ContainsKey((string)row[4]))
                                fk_definitions = definitions[(string)row[4]];

                            // Insert word to new database.
                            paramLanguageId.Value = row[1];
                            //if (fk_images > 0) paramImageId.Value = fk_images; else paramImageId.Value = null;
                            //if (fk_definitions > 0) paramDefinitionId.Value = fk_definitions; else paramImageId.Value = null;
                            paramImageId.Value = fk_images > 0 ? fk_images : (object)null;
                            paramDefinitionId.Value = fk_definitions > 0 ? fk_definitions : (object)null;
                            paramText.Value = row[4];
                            paramOccurrence.Value = row[5];
                            paramFuzzyHash.Value = row[6];
                            paramPhonetic.Value = row[7];

                            commandInsert.ExecuteNonQuery();


                            // Get new number.
                            int newID = Convert.ToInt32(commandNumber.ExecuteScalar());

                            // UPDATE in case the word was already existant
                            commandUpdate.ExecuteNonQuery();

                            words.Add(Convert.ToInt32(row[0]), newID);
                        }

                        transaction.Commit();
                    }

                    commandInsert.Dispose();
                    commandInsert = null;

                    commandNumber.Dispose();
                    commandNumber = null;

                    commandUpdate.Dispose();
                    commandUpdate = null;
                }


                //
                // 5) Words relationships
                //


                ReportDatabaseMigration(5.0 / 7, MigrationStrings[4]);

                // Add couples to the new DB.
                sql = "SELECT * FROM OldDB.pair WHERE is_updated = 1;";
                using (var command = new SqliteCommand(sql, db))
                {
                    DataTable table = new DataTable();
                    using (var reader = command.ExecuteReader())
                    {
                        table.Load(reader);
                        reader.Close();
                    }

                    int rows = table.Rows.Count;

                    string insertQuery = "INSERT OR IGNORE INTO pair(current_word_id, next_word_id, occurrence, is_updated) VALUES(:currentWordId, :nextWordId, :occurrence, 1);";
                    string selectQuery = "SELECT id FROM pair WHERE current_word_id = :currentWordId and next_word_id = :nextWordId";
                    string updateQuery = "UPDATE pair SET occurrence = :occurrence, is_updated = 1 WHERE current_word_id = :currentWordId and next_word_id = :nextWordId";

                    var commandInsert = new SqliteCommand(insertQuery, db);
                    SqliteParameter paramCurrentWordId = new SqliteParameter("currentWordId", db);
                    SqliteParameter paramNextWordId = new SqliteParameter("nextWordId", db);
                    SqliteParameter paramOccurrence = new SqliteParameter("occurrence", db);

                    commandInsert.Parameters.Add(paramCurrentWordId);
                    commandInsert.Parameters.Add(paramNextWordId);
                    commandInsert.Parameters.Add(paramOccurrence);

                    // Create SELECT number of the new word command.
                    var commandNumber = new SqliteCommand(selectQuery, db);

                    commandNumber.Parameters.Add(paramCurrentWordId);
                    commandNumber.Parameters.Add(paramNextWordId);

                    // Create UPDATE command
                    var commandUpdate = new SqliteCommand(updateQuery, db);

                    commandUpdate.Parameters.Add(paramCurrentWordId);
                    commandUpdate.Parameters.Add(paramNextWordId);
                    commandUpdate.Parameters.Add(paramOccurrence);

                    using (var transaction = db.BeginTransaction())
                    {
                        for (int i = 0; i < rows; i++)
                        {
                            double index = i + 1;
                            DataRow row = table.Rows[i];

                            ReportDatabaseMigration(5 / 7 + (index / rows), string.Format("{0} ({1}/{2})", MigrationStrings[4], index, rows));

                            int firstWord = Convert.ToInt32(row[1]);
                            int secondWord = Convert.ToInt32(row[2]);

                            if (words.ContainsKey(firstWord))
                                firstWord = words[firstWord];
                            if (words.ContainsKey(secondWord))
                                secondWord = words[secondWord];

                            paramCurrentWordId.Value = firstWord;
                            paramNextWordId.Value = secondWord;
                            paramOccurrence.Value = row[3];

                            commandInsert.ExecuteNonQuery();

                            // Get new number.
                            int newID = Convert.ToInt32(commandNumber.ExecuteScalar());

                            // UPDATE in case the pair was already existant
                            commandUpdate.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }

                    commandInsert.Dispose();
                    commandInsert = null;

                    commandNumber.Dispose();
                    commandNumber = null;

                    commandUpdate.Dispose();
                    commandUpdate = null;
                }


                //
                // X) Alternatives
                //
                // Migrate alternatives only for versions > 2.0 (starting 2.1)
                if (ConfigurationVersion >= Version.Parse("2.1.0.0"))
                {
                    // Copy alternatives to the new DB.
                    sql = "SELECT * FROM OldDB.alternative;";
                    using (var command = new SqliteCommand(sql, db))
                    {
                        DataTable table = new DataTable();
                        using (var reader = command.ExecuteReader())
                        {
                            table.Load(reader);
                            reader.Close();
                        }

                        string insertQuery = "INSERT OR IGNORE INTO alternative(word_id, text) VALUES(:wordId, :text);";

                        var paramWordId = new SqliteParameter("wordId", db);
                        var paramText = new SqliteParameter("text", db);

                        var commandInsert = new SqliteCommand(insertQuery, db);
                        commandInsert.Parameters.Add(paramWordId);
                        commandInsert.Parameters.Add(paramText);

                        using (var transaction = db.BeginTransaction())
                        {
                            // For each alternative...
                            for (int i = 0; i < table.Rows.Count; i++)
                            {
                                DataRow row = table.Rows[i];

                                // Insert new image into new database.
                                paramWordId.Value = row[1];
                                paramText.Value = row[2];
                                commandInsert.ExecuteNonQuery();

                            }

                            transaction.Commit();
                        }

                        commandInsert.Dispose();
                        commandInsert = null;
                    }
                }


                //
                // Detach database and close.
                //

                sql = "DETACH DATABASE 'OldDB';";
                using (var command = new SqliteCommand(sql, db))
                {
                    int result = command.ExecuteNonQuery();
                }

                db.Close();
            }

            // Force database release.
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();


            //
            // 6) Files update
            //


            ReportDatabaseMigration(6.0 / 7, MigrationStrings[5]);

            // Delete old database and its copy.
            try
            {
                if (File.Exists(_database_user_file))
                    File.Delete(_database_user_file);

                File.Move(newDatabase, _database_user_file);

                if (File.Exists(copyDatabase))
                    File.Delete(copyDatabase);
            }
            catch (Exception e)
            {
                _logService.Log(Log.LogModes.ERROR, "DesktopUpdateService", e.Message);
            }


            //
            // 7) Finish
            //

            ReportDatabaseMigration(1.0, MigrationStrings[6]);
        }

        private void ReportDatabaseMigration(double progression, string message = "")
        {
            DatabaseMigrationProgressChanged?.Invoke(progression * 100.0, message);
        }

        private bool VersionOlderThan(string version)
        {
            return CurrentVersion.CompareTo(OnlineVersion) < 0;
        }

        #endregion
    }
}
