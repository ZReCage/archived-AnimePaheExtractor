using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnimePaheExtractorWPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        SearchResults Results;

        public MainWindow() {
            InitializeComponent();
        }

        private async void Search_Click(object sender, RoutedEventArgs e) {
            SearchCriteria.IsEnabled = false;
            Results = await AnimepaheExtractor.Search(SearchCriteria.Text);
            SearchCriteria.IsEnabled = true;

            Grid gridResults = new Grid();
            gridResults.ColumnDefinitions.Add(new ColumnDefinition());

            foreach (Dictionary<string, string> _data in Results.Data) {
                string _v;
                _data.TryGetValue("_Title", out _v);
                TextBlock _t = new TextBlock();
                _t.Text = _v;
                
                gridResults.RowDefinitions.Add(new RowDefinition());
                gridResults.Children.Add(_t);
            }

            spSearchResults.Children.Add(gridResults);
        }

        private void OnKeyUpHandler(object sender, KeyEventArgs e) {
            if(e.Key == Key.Return)
                Search_Click(sender, e);
        }
    }
}
