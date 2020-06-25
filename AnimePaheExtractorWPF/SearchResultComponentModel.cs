using System.ComponentModel;

namespace AnimePaheExtractorWPF
{
    class SearchResultComponentModel : INotifyPropertyChanged
    {

        private string _id;
        private string _title;
        private string _type;
        private string _episodes;
        private string _season;

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        public string Slug { get; set; }
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }
        public string Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged("Type");
                }
            }
        }
        public string Episodes
        {
            get { return _episodes; }
            set
            {
                if (_episodes != value)
                {
                    _episodes = value;
                    OnPropertyChanged("Episodes");
                }
            }
        }
        public string Status { get; set; }
        public string Season
        {
            get { return _season; }
            set
            {
                if (_season != value)
                {
                    _season = value;
                    OnPropertyChanged("Season");
                }
            }
        }
        public string Score { get; set; }
        public string Image { get; set; }
        public string Relevance { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
