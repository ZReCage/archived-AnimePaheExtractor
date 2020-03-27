using System.ComponentModel;

namespace AnimePaheExtractorWPF {
    public class ExtractComponentModel : INotifyPropertyChanged {
        private string _title;
        public string Title {
            get { return _title; }
            set {
                if (_title != value) {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }
        private string _id;

        public string Id {
            get { return _id; }
            set {
                if (_id != value) {
                    _id = value;
                    // NotifyPropertyChanged("Id");
                }
            }
        }

        public string StatusBar { get; set; }

        StatusBarEnum _statusEnum;
        public StatusBarEnum StatusEnum {
            get => _statusEnum;

            set {
                if (_statusEnum != value) {
                    switch (value) {
                        case StatusBarEnum.PreparingChromium:
                            StatusBar = "Preparing Chromium";
                            break;

                        case StatusBarEnum.Launching:
                            StatusBar = "Launching";
                            break;

                        case StatusBarEnum.Ready:
                            StatusBar = "Ready to fight!";
                            break;

                        case StatusBarEnum.Error:
                            StatusBar = "There was an Error, contact support";
                            break;
                    }
                    OnPropertyChanged("StatusBar");

                    _statusEnum = value;
                }
            }
        }

        public enum StatusBarEnum {
            Null,
            PreparingChromium,
            Launching,
            Ready,
            Error
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
