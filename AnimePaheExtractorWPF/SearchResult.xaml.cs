using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

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
                    searchResultCM.Episodes = value;
                    ToTextBox.Text = value;

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
        public string Image {
            get { return searchResultCM.Image; }
            set {
                if (searchResultCM.Image != value) {
                    searchResultCM.Image = value;
                }
            }
        }

        private void SearchResultDropDown_Click(object sender, RoutedEventArgs e) {
            ExtractOptions.Visibility = Visibility.Visible;
        }
    }
}