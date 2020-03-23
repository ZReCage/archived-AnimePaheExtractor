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

        public static async void ReadyToExtract(Dictionary<string, string> _d, int from, int to) {
            Task<bool> init = AnimepaheExtractor.InitializePuppeteer();

            Extract _extract = new Extract();
            _extract.SerieToDownload = _d;
            _mainWindow.ExtractTabItem.Content = _extract;
            _mainWindow.MainMenuTabControl.SelectedIndex = 2;

            IList<string> _sessionList;

            _d.TryGetValue("id", out string s_id);
            int _id = Convert.ToInt32(s_id);
            _sessionList = await AnimepaheExtractor.GetEpisodesList(_id, from, to);

            try { 
                EpisodesReadyToExtract.MultiLinksClass.Clear();
            } catch {
                EpisodesReadyToExtract.MultiLinksClass = new List<EpisodesReadyToExtract.Links>();
            }

            foreach (string _session in _sessionList) {
                var _links = new EpisodesReadyToExtract.Links();
                IList<EpisodeExtractLink> episodeExtractLinks;

                episodeExtractLinks = await AnimepaheExtractor.GetEpisodeExtractLink(_id, _session);
                _links.LinkList = episodeExtractLinks;

                EpisodesReadyToExtract.MultiLinksClass.Add(_links);
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
    }
}