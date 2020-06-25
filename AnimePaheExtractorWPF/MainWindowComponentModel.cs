using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AnimePaheExtractorWPF
{
    public class MainWindowComponentModel : INotifyPropertyChanged
    {
        public void Startup()
        {
            // Jokes keep my mind calm, that and Animes/Games OST while I code garbage. I like lofi lately as well
            SetStatusBar(StatusBarEnum.Default);
    
            string rawDataURL = "https://pastebin.com" + "/raw/" + "pKZLJWPe";
            string rawData = AnimepaheExtractor.GetRequest(rawDataURL, 3);

            if (rawData != string.Empty)
            {
                Thread.Sleep(1000);
                var dataArray = rawData.Split('\n');
                Random _random = new Random();

                int rIndex = _random.Next(dataArray.Length);
                StatusBar = dataArray[rIndex];
            }
            else
            {
                StatusBar = "Nothing to see here :(";
            }
            Application.Current.Dispatcher.Invoke(() => MainWindow._mainWindow.ContinueToSearch.IsEnabled = true);
        }

        private StatusBarEnum _statusEnum;
        public bool SetStatusBar(StatusBarEnum _newStatusEnum)
        {
            if (_statusEnum != _newStatusEnum)
            {
                switch (_newStatusEnum)
                {
                    case StatusBarEnum.Null:
                        StatusBar = "";
                        break;
                    case StatusBarEnum.Default:
                        StatusBar = "Gathering garbage...";
                        break;
                    case StatusBarEnum.PreparingChromium:
                        StatusBar = "Preparing Chromium...";
                        break;

                    case StatusBarEnum.Launching:
                        StatusBar = "Launching Chromium...";
                        break;

                    case StatusBarEnum.Ready:
                        StatusBar = "Chromium is readier than ever";
                        break;

                    case StatusBarEnum.Error:
                        StatusBar = "There was an error :(";
                        break;
                }
                _statusEnum = _newStatusEnum;

                return true;
            }

            return false;
        }
        private string _statusBar;
        public string StatusBar
        {
            get { return _statusBar; }
            set
            {
                if (_statusBar != value)
                {
                    _statusBar = value;
                    OnPropertyChanged("StatusBar");
                }
            }
        }

        public enum StatusBarEnum
        {
            Null,
            PreparingChromium,
            Launching,
            Ready,
            Error,
            Default
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
