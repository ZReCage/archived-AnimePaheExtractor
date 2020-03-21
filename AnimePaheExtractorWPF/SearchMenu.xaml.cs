using System;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AnimePaheExtractorWPF {
    /// <summary>
    /// Interaction logic for SearchMenu.xaml
    /// </summary>
    public partial class SearchMenu : UserControl {
        SearchResults Results;
        public SearchMenu() {
            InitializeComponent();

        }
        private async void Search_Click(object sender, RoutedEventArgs e) {
            if (SearchCriteria.Text.Length > 3) {

                SearchCriteria.IsEnabled = false;
                SearchButton.IsEnabled = false;

                Results = await AnimepaheExtractor.Search(SearchCriteria.Text);

                SearchCriteria.IsEnabled = true;
                SearchButton.IsEnabled = true;

                IList < SearchResult > searchResultsList = new List<SearchResult>();

                SearchResultsStackPanel.Children.Clear();
                if (Results.Total > 0) {

                    foreach (Dictionary<string, string> _data in Results.Data) {

                        _data.TryGetValue("season", out string _season);
                        _data.TryGetValue("title", out string _title);
                        _data.TryGetValue("type", out string _type);
                        _data.TryGetValue("episodes", out string _episodes);
                        _data.TryGetValue("image", out string _image);

                        SearchResult _r = new SearchResult {
                            Title = _title,
                            Type = _type,
                            Episodes = _episodes,
                            Season = _season,
                            Image = _image
                        };

                        /*
                            Values:
                                id
                                slug
                                title
                                type
                                episodes
                                status
                                season
                                score
                                image
                                relevance
                         */

                        BitmapImage _bitmap = new BitmapImage();
                        _bitmap.BeginInit();
                        _bitmap.UriSource = new Uri(_r.Image);
                        _bitmap.EndInit();

                        SearchResultsImage.Source = _bitmap;

                        SearchResultsStackPanel.Children.Add(_r);
                    }
                } else {

                }
                UpdateLayout();
            }

        }

        private void OnKeyUpHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return)
                Search_Click(sender, e);
        }
    }
}
