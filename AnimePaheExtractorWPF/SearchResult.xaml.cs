using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AnimePaheExtractorWPF {
    /// <summary>
    /// Interaction logic for SearchResult.xaml
    /// </summary>
    public partial class SearchResult : UserControl {
        private SearchResultComponentModel searchResultCM = null;
        public SearchResult() {
            InitializeComponent();

            searchResultCM = new SearchResultComponentModel();
            this.DataContext = searchResultCM;
        }

        public string Id {
            get { return searchResultCM.Id; }
            set {
                if (searchResultCM.Id != value) {
                    searchResultCM.Id = value;
                }
            }
        }

        public string Title {
            get { return searchResultCM.Title; }
            set {
                if (searchResultCM.Title != value) {
                    searchResultCM.Title = value;
                }
            }
        }
        public string Episodes {
            get { return searchResultCM.Episodes; }
            set {
                if (searchResultCM.Episodes != value) {
                    if(Convert.ToInt32(value) > 0) {
                        searchResultCM.Episodes = value;
                        ToTextBox.Text = value;
                    } else {
                        searchResultCM.Episodes = "?";
                        ToTextBox.Text = "Unk";
                    }

                }
            }
        }
        public string Type {
            get { return searchResultCM.Type; }
            set {
                if (searchResultCM.Type != value) {
                    searchResultCM.Type = value;
                }
            }
        }
        public string Season {
            get { return searchResultCM.Season; }
            set {
                if (searchResultCM.Season != value) {
                    searchResultCM.Season = value;
                }
            }
        }
        public BitmapImage Image { get; set; }
    }
}