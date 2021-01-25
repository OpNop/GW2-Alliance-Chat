using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Chat_Client.Utils
{
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Guild Wars 2\addons\TACS\settings.json");
        private string _APIKey;
        private bool _EnableDiscord;
        private bool _ShowJoin;
        private bool _ShowOnMap;
        private bool _ShowLeave;
        private bool _ShowTimestamp;
        private double _ChatTop;
        private double _ChatLeft;
        private double _ChatHeight;
        private double _ChatWidth;

        [DefaultValue("")]
        public string APIKey
        {
            get { return _APIKey; }
            set
            {
                _APIKey = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(false)]
        public bool EnableDiscord
        {
            get { return _EnableDiscord; }
            set
            {
                _EnableDiscord = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(true)]
        public bool ShowJoin
        {
            get { return _ShowJoin; }
            set
            {
                _ShowJoin = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(false)]
        public bool ShowOnMap
        {
            get { return _ShowOnMap; }
            set
            {
                _ShowOnMap = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(true)]
        public bool ShowLeave
        {
            get { return _ShowLeave; }
            set
            {
                _ShowLeave = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(true)]
        public bool ShowTimestamp
        {
            get { return _ShowTimestamp; }
            set
            {
                _ShowTimestamp = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(0)]
        public double ChatTop
        {
            get { return _ChatTop; }
            set
            {
                _ChatTop = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(0)]
        public double ChatLeft
        {
            get { return _ChatLeft; }
            set
            {
                _ChatLeft = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(0)]
        public double ChatHeight
        {
            get { return _ChatHeight; }
            set
            {
                _ChatHeight = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(0)]
        public double ChatWidth
        {
            get { return _ChatWidth; }
            set
            {
                _ChatWidth = value;
                OnPropertyChanged();
            }
        }

        public Settings Load()
        {
            string reader = "";
            try
            {
                reader = File.ReadAllText(SettingsPath);
            }
            catch (FileNotFoundException)
            {
                //Settings not found, using defaults
                Console.WriteLine("settings.json is missing. Using defualts");
                Save();
                Load();
            }

            var result = JsonConvert.DeserializeObject<Settings>(reader, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate });
            return result;
        }

        public void Save()
        {
            try
            {
                var settingsJson = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsPath, settingsJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
