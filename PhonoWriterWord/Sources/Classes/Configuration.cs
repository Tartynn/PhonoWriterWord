using PhonoWriterWord.Values;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace PhonoWriterWord.Sources.Classes
{
	public class Configuration : ICloneable
	{
		#region Properties

		public string Version { get; set; }

		// General
		public bool ActivateLogs { get; set; }

		// Interface Lang & DB Lang
		public int InterfaceLanguage { get; set; }
		public int Language { get; set; }

		// Engines
		public string EngineName { get; set; }
		public bool UseSequence { get; set; }
		public List<string> Sequence { get; set; }
		public string VoiceName { get; set; }
		public int VoiceVolume { get; set; }
		public int VoiceSpeed { get; set; }

		// Startup
		public bool SplashScreenAtStartup { get; set; }
		public bool WhatsNewAtStartup { get; set; }
		public bool StartMinimized { get; set; }
		public bool StartWithWindows { get; set; }
		public bool CheckForUpdatesAtStartup { get; set; }
		public bool CenterPredictionsAtStartup { get; set; }

		// Predictions
		public bool ClassicPredictionActivated { get; set; }
		public int ClassicPredictionsMinCharNumber { get; set; }
		public int ClassicPredictionsNumber { get; set; }
		public bool PhoneticPredictionActivated { get; set; }
		public int PhoneticMinCharNumber { get; set; }
		public int PhoneticMaxCharNumber { get; set; }
		public int PhoneticPredictionNumber { get; set; }
		public bool FuzzyPredictionActivated { get; set; }
		public int FuzzyPredictionNumber { get; set; }
		public bool PictographicPrediction { get; set; }
		public bool PictographicHidePictureless { get; set; }
		public bool RemoveEszetts { get; set; }

		// Main window
		public bool AutomaticPaste { get; set; }
		public string Font { get; set; }
		public int FontSize { get; set; }
		public bool Reading { get; set; }
		public bool ShowEditor { get; set; }

		// Predictions window
		public bool PredictionsAudition { get; set; }
		public bool PredictionsDefinition { get; set; }
		public bool ShowButtonPicture { get; set; }
		public bool ShowButtonDefinition { get; set; }
		public bool ShowButtonAudition { get; set; }
		public bool ShowButtonReading { get; set; }
		public bool ShowButtonConfiguration { get; set; }
		public bool ShowWordEditbutton { get; set; }
		public bool UseArrowKeys { get; set; }
		public bool UseFunctionButtons { get; set; }

		// Events
		public event EventHandler ConfigurationChanged;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor, used to set default values to every property
		/// </summary>
		private Configuration()
		{
			Version = "0";

			// General
			ActivateLogs = true;

			// Interface Lang & DB Lang (using system's language)
			//InterfaceLanguage = LanguageUtils.ConvertLanguageToInt(CultureInfo.InstalledUICulture.TwoLetterISOLanguageName);
			//Language = InterfaceLanguage;

			// Engines
			EngineName = string.Empty;
			UseSequence = false;
			Sequence = new List<string>();
			VoiceName = "Microsoft Server Speech Text to Speech Voice (fr-FR, Hortense)";
			VoiceVolume = 50;
			VoiceSpeed = 0;

			// Startup
			SplashScreenAtStartup = true;
			WhatsNewAtStartup = true;
			StartMinimized = false;
			StartWithWindows = false;
			CheckForUpdatesAtStartup = false;
			CenterPredictionsAtStartup = false;

			// Predictions
			ClassicPredictionActivated = true;
			ClassicPredictionsNumber = 3;
			ClassicPredictionsMinCharNumber = 1;
			PhoneticPredictionActivated = true;
			PhoneticMinCharNumber = 1;
			PhoneticPredictionNumber = 3;
			PhoneticMaxCharNumber = 50;
			FuzzyPredictionActivated = true;
			FuzzyPredictionNumber = 3;
			PictographicPrediction = true;
			PictographicHidePictureless = false;
			RemoveEszetts = true;

			// Main window
			AutomaticPaste = false;
			Font = System.Drawing.SystemFonts.DefaultFont.Name;
			FontSize = 16;
			Reading = true; // Listen what the user is typing.
			ShowEditor = true;

			// Predictions window
			PredictionsAudition = false;
			PredictionsDefinition = true;
			ShowButtonPicture = true;
			ShowButtonDefinition = true;
			ShowButtonAudition = true;
			ShowButtonReading = true;
			ShowButtonConfiguration = true;
			ShowWordEditbutton = true;
			UseArrowKeys = false;
			UseFunctionButtons = false;
		}

		#endregion

		#region Methods

		public object Clone()
		{
			Configuration config = new Configuration();

			config.Version = Version;

			// General
			config.ActivateLogs = ActivateLogs;

			// Interface Lang & DB Lang
			config.InterfaceLanguage = InterfaceLanguage;
			config.Language = Language;

			// Engines
			config.EngineName = EngineName;
			config.UseSequence = UseSequence;
			foreach (var item in Sequence)
				config.Sequence.Add(item);
			config.VoiceName = VoiceName;
			config.VoiceVolume = VoiceVolume;
			config.VoiceSpeed = VoiceSpeed;

			// Startup
			config.SplashScreenAtStartup = SplashScreenAtStartup;
			config.WhatsNewAtStartup = WhatsNewAtStartup;
			config.StartMinimized = StartMinimized;
			config.StartWithWindows = StartWithWindows;
			config.CheckForUpdatesAtStartup = CheckForUpdatesAtStartup;
			config.CenterPredictionsAtStartup = CenterPredictionsAtStartup;

			// Predictions
			config.ClassicPredictionActivated = ClassicPredictionActivated;
			config.ClassicPredictionsNumber = ClassicPredictionsNumber;
			config.ClassicPredictionsMinCharNumber = ClassicPredictionsMinCharNumber;
			config.PhoneticPredictionActivated = PhoneticPredictionActivated;
			config.PhoneticMinCharNumber = PhoneticMinCharNumber;
			config.PhoneticPredictionNumber = PhoneticPredictionNumber;
			config.PhoneticMaxCharNumber = PhoneticMaxCharNumber;
			config.FuzzyPredictionActivated = FuzzyPredictionActivated;
			config.FuzzyPredictionNumber = FuzzyPredictionNumber;
			config.PictographicPrediction = PictographicPrediction;
			config.PictographicHidePictureless = PictographicHidePictureless;

			// Main window
			config.AutomaticPaste = AutomaticPaste;
			config.Font = Font;
			config.FontSize = FontSize;
			config.Reading = Reading;
			config.ShowEditor = ShowEditor;

			// Predictions window
			config.ShowButtonPicture = ShowButtonPicture;
			config.ShowButtonDefinition = ShowButtonDefinition;
			config.ShowButtonAudition = ShowButtonAudition;
			config.ShowButtonReading = ShowButtonReading;
			config.ShowButtonConfiguration = ShowButtonConfiguration;
			config.ShowWordEditbutton = ShowWordEditbutton;
			config.UseArrowKeys = UseArrowKeys;
			config.UseFunctionButtons = UseFunctionButtons;


			return config;
		}

		/// <summary>
		/// Load default config into instance.
		/// </summary>
		/// <returns>Has config been loaded ?</returns>
		public static Configuration Open(string path)
		{
			// Skip if config doesn't exist already.
			if (!File.Exists(path))
				return new Configuration();

			Configuration configuration = null;
			XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
			FileStream fs = new FileStream(path, FileMode.Open);

			// Avoid loading invalid XML.
			try
			{
				configuration = (Configuration)serializer.Deserialize(fs);
			}
			catch (Exception e)
			{
				Console.WriteLine("Open::Exception catched : " + e.StackTrace);
			}

			fs.Close();

			return configuration;
		}

		public void Save()
		{
			//Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() =>
			Dispatcher.CurrentDispatcher.BeginInvoke(new Action(delegate ()
			{
				if (!Directory.Exists(Constants.LOCAL_APP_PATH))
				{
					FileSystemRights rights = FileSystemRights.FullControl;
					Directory.CreateDirectory(Constants.LOCAL_APP_PATH);
					DirectoryInfo directoryInfo = new DirectoryInfo(Constants.LOCAL_APP_PATH);
					DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
					SecurityIdentifier identifier = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
					FileSystemAccessRule rule = new FileSystemAccessRule(identifier, rights, InheritanceFlags.ObjectInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
					directorySecurity.AddAccessRule(rule);
					directoryInfo.SetAccessControl(directorySecurity);
				}

				// Create an instance of the XmlSerializer class;// specify the type of object to serialize.
				XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
				TextWriter writer = new StreamWriter(Constants.CONFIG_FILE);
				serializer.Serialize(writer, this);
				writer.Close();

				//Publish the event that the configuration has been changed
				ConfigurationChanged?.Invoke(this, new EventArgs());
			}));
		}

		#endregion
	}
}