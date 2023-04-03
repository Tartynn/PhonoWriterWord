using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Values
{
    class Constants
    {
        // Strings
        public static readonly string PAYPAL_GRE10_URL = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=8J4XV6ASJRUR8";
        public static readonly string PAYPAL_URL = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=5K9ULP7626CGQ";

        // Resources
        public static readonly string STRINGS_RESOURCES = "Strings";
        public static readonly string UPDATE_RESOURCES = "Update";

        // Folders
        public static readonly string EXECUTABLE_PATH = Assembly.GetExecutingAssembly().Location; // EXE file path
        public static readonly string BINARY_PATH = Path.GetDirectoryName(EXECUTABLE_PATH); // EXE folder
        public static readonly string APPLICATION_PATH = Directory.GetParent(BINARY_PATH).FullName; // Project folder
        public static readonly string LOCAL_APP_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PhonoWriter");
        //public static readonly string COMMON_APP_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PhonoWriter");
        public static readonly string ENGINES_DIRECTORY_PATH = Path.Combine(APPLICATION_PATH, "Engines");
        public static readonly string IMAGES_CUSTOM_PATH = Path.Combine(LOCAL_APP_PATH, "Images");
        public static readonly string IMAGES_PATH = Path.Combine(APPLICATION_PATH, "Images");

        //Folders Updated
        public static readonly string solutionDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        public static readonly string COMMON_APP_PATH = Path.Combine(solutionDirectory, "Database", "Files");
        public static readonly string IMAGES = Path.Combine(solutionDirectory, "Images");

        // File
        public static readonly string CONFIG_FILE = Path.Combine(LOCAL_APP_PATH, "config.xml");
        //public static readonly string DATABASE_BASE_FILE = Path.Combine(COMMON_APP_PATH, "dictionary.db"); // Original database.
        //public static readonly string DATABASE_FILE = Path.Combine(LOCAL_APP_PATH, "dictionary.db"); // User's database (copy of original).
        //public static readonly string DATABASE_FILE_NEW = Path.Combine(LOCAL_APP_PATH, "dictionary.db.new"); // New database replacing user's.
        //public static readonly string DATABASE_FILE_COPY = Path.Combine(LOCAL_APP_PATH, "dictionary.db.copy"); // Copy of old database (to avoid file locking issues).
        public static readonly string ICON_FILE = Path.Combine(IMAGES_PATH, "icon.ico");
        public static readonly string LOG_FILE = Path.Combine(LOCAL_APP_PATH, "log.txt");


        //Files Updated
        public static readonly string DATABASE_BASE_FILE = Path.Combine(COMMON_APP_PATH, "dictionary.db"); // Original database.
        public static readonly string DATABASE_FILE = Path.Combine(COMMON_APP_PATH, "Custom", "dictionary.db"); // User's database (copy of original).
        public static readonly string DATABASE_FILE_NEW = Path.Combine(COMMON_APP_PATH, "Custom", "dictionary.db.new"); // New database replacing user's.
        public static readonly string DATABASE_FILE_COPY = Path.Combine(COMMON_APP_PATH, "Custom", "dictionary.db.copy"); // Copy of old database (to avoid file locking issues).

    }
}
