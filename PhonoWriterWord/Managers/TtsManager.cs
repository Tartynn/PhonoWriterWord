using PhonoWriterWord;
using PhonoWriterWord.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

public class TtsManager
{
    private readonly SpeechSynthesizer _speechSynthesizer;

    public TtsManager()
    {
        _speechSynthesizer = new SpeechSynthesizer();
        SetVoiceForCurrentLanguage();
    }

    public void Speak(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _speechSynthesizer.SpeakAsyncCancelAll();
            _speechSynthesizer.SpeakAsync(text);
        }
    }

    public void SetVoice(string voice)
    {
        var installedVoices = _speechSynthesizer.GetInstalledVoices();
        var selectedVoice = installedVoices.FirstOrDefault(v => v.VoiceInfo.Culture.Name == voice);

        if (selectedVoice != null)
        {
            _speechSynthesizer.SelectVoice(selectedVoice.VoiceInfo.Name);
        }
        else
        {
            // Show a message to the user that the required voice is not installed
            MessageBox.Show($"The required voice '{voice}' is not installed on this machine. Please install the appropriate voice.", "Voice not installed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public void SetVolume(int volume)
    {
        _speechSynthesizer.Volume = volume;
    }

    public void SetRate(int rate)
    {
        _speechSynthesizer.Rate = rate;
    }

    public void Stop()
    {
        _speechSynthesizer.SpeakAsyncCancelAll();
    }
    public void SetVoiceForCurrentLanguage()
    {
        string twoLetterISOLanguageName = "fr"; // Replace IsoCode with the appropriate property name

        if (Globals.ThisAddIn.LanguagesManager != null)
        {
            twoLetterISOLanguageName = Globals.ThisAddIn.LanguagesManager.CurrentLanguage.Iso; // Replace IsoCode with the appropriate property name

        }

        int languageCodeInt = LanguageUtils.ConvertLanguageToInt(twoLetterISOLanguageName);
        string languageCodeString = LanguageUtils.ConvertLanguageToString(languageCodeInt, false);
        SetVoice(languageCodeString);
    }
    public void SetLanguage(string language)
    {
        CultureInfo culture = new CultureInfo(language);
        string cultureName = culture.Name;

        foreach (var voice in _speechSynthesizer.GetInstalledVoices())
        {
            if (voice.VoiceInfo.Culture.Name.Equals(cultureName, StringComparison.OrdinalIgnoreCase))
            {
                _speechSynthesizer.SelectVoice(voice.VoiceInfo.Name);
                break;
            }
        }
    }
}

