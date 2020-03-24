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
        static MainWindow _mainWindow;

        public MainWindow() {
            _mainWindow = this;
            InitializeComponent();
        }

        public static async void ReadyToExtract(Serie _serie, Range _range = null) {
            Task<bool> init = AnimepaheExtractor.InitializePuppeteer();

            Extract.CurrentSerie = _serie;

            Extract _extract = new Extract();
            _mainWindow.ExtractTabItem.Content = _extract;
            _mainWindow.MainMenuTabControl.SelectedIndex = 2;

            Extract.CurrentEpisodes = await AnimepaheExtractor.GetEpisodesList(Extract.CurrentSerie.Id, _range);


            foreach(Episode _episode in Extract.CurrentEpisodes) {
                bool _gathered = await _episode.GatherEpisodeLinksData(Extract.CurrentSerie.Id);

                if( _gathered ) {
                    ExtractListViewItems _items = new ExtractListViewItems() {
                        Number = _episode.EpisodeNumber,
                        Quality = _episode.EpisodeLinksData[0].Quality,
                        FanSub = _episode.EpisodeLinksData[0].FanSub,
                        Progress = 0
                    };
                
                    _extract.ExtractListView.Items.Add(_items);
                }
            }

            await init;
        }

        public static bool IsSearchTabEnabled {
            get{ return _mainWindow.SearchTabItem.IsEnabled; }
            set{
                _mainWindow.SearchTabItem.IsEnabled = value;
            }
        }

        private void GoToSearch_Click(object sender, RoutedEventArgs e) {
            SearchTabItem.IsSelected = true;
        }

        public class ExtractListViewItems {
            public double Number { get; set; }
            public int Quality { get; set; }
            public string FanSub { get; set; }
            public int Progress { get; set; }
        }
    }
}