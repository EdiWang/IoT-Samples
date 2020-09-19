using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace PiCameraSurveillanceClient
{
    public class AppSettings : INotifyPropertyChanged
    {
        public string StorageAccountName
        {
            get
            {
                return ReadSettings(nameof(StorageAccountName), string.Empty);
            }
            set
            {
                SaveSettings(nameof(StorageAccountName), value);
                NotifyPropertyChanged();
            }
        }

        public string StorageAccountKey
        {
            get
            {
                return ReadSettings(nameof(StorageAccountKey), string.Empty);
            }
            set
            {
                SaveSettings(nameof(StorageAccountKey), value);
                NotifyPropertyChanged();
            }
        }

        public string StorageContainerName
        {
            get
            {
                return ReadSettings(nameof(StorageContainerName), string.Empty);
            }
            set
            {
                SaveSettings(nameof(StorageContainerName), value);
                NotifyPropertyChanged();
            }
        }

        public ApplicationDataContainer SettingsContainer { get; set; }

        public AppSettings()
        {
            SettingsContainer = ApplicationData.Current.LocalSettings;
        }

        private void SaveSettings(string key, object value)
        {
            SettingsContainer.Values[key] = value;
        }

        private T ReadSettings<T>(string key, T defaultValue)
        {
            if (SettingsContainer.Values.ContainsKey(key))
            {
                return (T)SettingsContainer.Values[key];
            }
            if (null != defaultValue)
            {
                return defaultValue;
            }
            return default(T);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName]string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
