using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace GraphicsEngine
{
    public enum language { Ru, En }

    public class UILanguageManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Hashtable Controls      { get { return ReadFileToProperty(nameof(Controls)); } }
        public Hashtable ErrorMessages { get { return ReadFileToProperty(nameof(ErrorMessages)); } }

        private language currentLanguage;
        public  language CurrentLanguage { get { return currentLanguage; } 
            set 
            {
                currentLanguage = value;
                if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("")); }
            } 
        }
        
        public UILanguageManager (language language = language.En) => CurrentLanguage = language;

        Hashtable ReadFileToProperty (string fileName)
        {
            var path = $"Language files/{Enum.GetName(typeof(language), CurrentLanguage)}/";
            try
            {
                var serialized = File.ReadAllText(path + $"{fileName}.json");
                return JsonConvert.DeserializeObject<Hashtable>(serialized);
            }
            catch { throw new ArgumentException($"{fileName}.json file in {path} is missing or corrupted"); }
        }
    }
}