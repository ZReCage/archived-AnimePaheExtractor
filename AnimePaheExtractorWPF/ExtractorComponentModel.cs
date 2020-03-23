using System.ComponentModel;

namespace AnimePaheExtractorWPF {
    class ExtractorComponentModel : INotifyPropertyChanged {


        private string _title;
        public string Title {
            get { return _title; }
            set {
                if (_title != value) {
                    _title = value;
                    NotifyPropertyChanged("Title");
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
