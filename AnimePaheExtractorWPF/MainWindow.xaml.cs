using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class MainWindow : Window {
        static MainWindow _mainWindow;
        static Extract Extract;

        public MainWindow() {
            _mainWindow = this;
            InitializeComponent();
        }

        public static async void ReadyToExtract(Serie _serie, Range _range = null) {
            MainWindow.IsSearchTabEnabled = false;
            AnimePaheExtractorWPF.Extract.CurrentSerie = _serie;

            Extract Extract = new Extract();
            _mainWindow.ExtractTabItem.Content = Extract;

            _mainWindow.MainMenuTabControl.SelectedIndex = 2;

            AnimepaheExtractor.InitializePuppeteer(Extract);

            IList<Episode> _episodes = await AnimepaheExtractor.GetEpisodesList(Extract.CurrentSerie.Id, _range);

            /////////////////////////////////////////Extract.CurrentExtractListViewItems = new List<ExtractListViewItem>();

            foreach (Episode _episode in _episodes) {
                bool _gathered = await _episode.GatherEpisodeLinksData(Extract.CurrentSerie.Id);

                if( _gathered ) {
                    ExtractGridItem _item = new ExtractGridItem() {
                        EpisodeNumber = _episode.EpisodeNumber,
                        Quality = _episode.EpisodeLinksData[0].Quality,
                        FanSub = _episode.EpisodeLinksData[0].FanSub,
                        Progress = 0,
                        StatusEnum = ExtractionStatus.Queued,

                        Episode = _episode
                    };

                    //////////////////////////////////////////////Extract.CurrentExtractListViewItems.Add(_item);
                    Extract.ExtractsGrid.DataContext = _item;

                    Extract.ExtractsGrid.Items.Add(_item);
                    
                    Extract.ExtractsGrid_AddItem(_item);
                }
            }
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

        void DataWindow_Closing(object sender, CancelEventArgs e) {
            AnimepaheExtractor.FinishPuppeteer();
            if (Extract != null && Extract.Downloader != null) {
                Extract.Downloader.StopDownload();
            }
        }
    }
    public class ExtractGridItem : INotifyPropertyChanged {
        public double EpisodeNumber { get; set; }
        public int Quality { get; set; }
        public string FanSub { get; set; }

        private int _progress;
        public int Progress {
            get => _progress;
            set {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public string Status { get; set; }

        private ExtractionStatus _statusEnum;
        public ExtractionStatus StatusEnum {
            get => _statusEnum;

            set {
                if (_statusEnum != value) {
                    switch (value) {
                        case ExtractionStatus.Queued:
                            Status = "Queued";
                            break;

                        case ExtractionStatus.Starting:
                            Status = "Starting";
                            break;

                        case ExtractionStatus.Downloading:
                            Status = "Downloading";
                            break;

                        case ExtractionStatus.Completed:
                            Status = "Completed";
                            break;

                        case ExtractionStatus.Error:
                            Status = "There was an error";
                            break;
                    }
                    OnPropertyChanged("Status");

                    _statusEnum = value;
                }
            }

        }

        public Episode Episode;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public enum ExtractionStatus {
        Null,
        Queued,
        Starting,
        Downloading,
        Completed,
        Error
    }
}