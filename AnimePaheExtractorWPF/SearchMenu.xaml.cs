using System;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AnimePaheExtractorWPF
{
    /// <summary>
    /// Interaction logic for SearchMenu.xaml
    /// </summary>
    public partial class SearchMenu : UserControl
    {
        SearchResults Results;
        SearchResult LastSearchResultClicked;

        Image Poster = new Image();

        public SearchMenu()
        {
            Grid.SetColumn(Poster, 1);
            Poster.VerticalAlignment = VerticalAlignment.Center;
            Poster.HorizontalAlignment = HorizontalAlignment.Center;
            Poster.Margin = new Thickness(5, 0, 0, 0);
            Poster.Stretch = Stretch.Uniform;

            InitializeComponent();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            if (SearchCriteria.Text.Length > 3)
            {

                SearchCriteria.IsEnabled = false;
                SearchButton.IsEnabled = false;

                Results = await AnimepaheExtractor.Search(SearchCriteria.Text);

                SearchCriteria.IsEnabled = true;
                SearchButton.IsEnabled = true;

                IList<SearchResult> searchResultsList = new List<SearchResult>();

                SearchResultsStackPanel.Children.Clear();
                SearchResultPreview.Children.Clear();

                if (Results.Total > 0)
                {

                    foreach (Dictionary<string, string> _data in Results.Data)
                    {

                        _data.TryGetValue("id", out string _id);
                        _data.TryGetValue("title", out string _title);
                        _data.TryGetValue("type", out string _type);
                        _data.TryGetValue("episodes", out string _episodes);
                        _data.TryGetValue("season", out string _season);
                        _data.TryGetValue("poster", out string _uriImage);

                        SearchResult _r = new SearchResult
                        {
                            Id = _id,
                            Title = _title,
                            Type = _type,
                            Episodes = _episodes,
                            Season = _season,
                        };

                        _r.SearchResultDropDown.Click += (s, ev) =>
                        {
                            _r.ExtractOptions.Visibility = Visibility.Visible;
                            Poster.Source = _r.Image;

                            if (LastSearchResultClicked != null && LastSearchResultClicked != _r)
                                LastSearchResultClicked.ExtractOptions.Visibility = Visibility.Collapsed;

                            LastSearchResultClicked = _r;
                        };

                        _r.StartExtraction.Click += (s, ev) =>
                        {
                            Serie _serie = new Serie(_r.Title, Convert.ToInt32(_r.Id));

                            if (_r.ExtractAllRadioButton.IsChecked == true)
                                MainWindow.ReadyToExtract(_serie);
                            else
                            {
                                Range _range = new Range()
                                {
                                    From = Convert.ToInt32(_r.FromTextBox.Text),
                                    To = Convert.ToInt32(_r.ToTextBox.Text)
                                };

                                MainWindow.ReadyToExtract(_serie, _range);
                            }
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

                        _r.Image = new BitmapImage();
                        _r.Image.BeginInit();
                        _r.Image.UriSource = new Uri(_uriImage);
                        _r.Image.EndInit();

                        SearchResultsStackPanel.Children.Add(_r);
                    }

                    SearchResultPreview.Children.Add(Poster);
                }
                else
                {
                    TextBlock _tError = new TextBlock
                    {
                        Text = "Nothing was found.",
                        FontSize = 32,
                        Foreground = Brushes.White
                    };

                    TextBlock _tHelp = new TextBlock
                    {
                        Text = "Maybe it's just a typo (I'm used to it). Try another search criteria.",
                        FontSize = 18,
                        Foreground = Brushes.DarkGray
                    };

                    SearchResultsStackPanel.Children.Add(_tError);
                    SearchResultsStackPanel.Children.Add(_tHelp);
                }
                UpdateLayout();
            }

        }

        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                Search_Click(sender, e);
        }
    }
}
