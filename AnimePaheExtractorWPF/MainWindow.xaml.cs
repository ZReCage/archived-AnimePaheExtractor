using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace AnimePaheExtractorWPF
{
    public partial class MainWindow : Window
    {
        public static MainWindow _mainWindow;
        public static MainWindowComponentModel mainWindowCM = new MainWindowComponentModel();
        public static Extract DefExtract = null;

        public MainWindow()
        {
            DataContext = mainWindowCM;
            
            _mainWindow = this;
            InitializeComponent();
        }

        public static async void ReadyToExtract(Serie _serie, Range _range = null)
        {
            AnimePaheExtractorWPF.Extract.CurrentSerie = _serie;

            DefExtract = new Extract();
            _mainWindow.ExtractTabItem.Content = DefExtract;

            _mainWindow.MainMenuTabControl.SelectedIndex = 2;

            // Bring puppeteer to life, from the nothing it's become
            AnimepaheExtractor.InitializePuppeteer();

            IList<Episode> _episodes = await AnimepaheExtractor.GetEpisodesList(Extract.CurrentSerie.Id, _range);

            foreach (Episode _episode in _episodes)
            {
                bool _gathered = await _episode.GatherEpisodeLinksData(Extract.CurrentSerie.Id);

                if (_gathered)
                {
                    ExtractGridItem _item = new ExtractGridItem()
                    {
                        EpisodeNumber = _episode.EpisodeNumber,
                        Quality = _episode.EpisodeLinksData[0].Quality,
                        FanSub = _episode.EpisodeLinksData[0].FanSub,
                        Progress = 0,
                        StatusEnum = ExtractionStatus.Queued,

                        Episode = _episode
                    };

                    DefExtract.ExtractsGrid.DataContext = _item;

                    DefExtract.ExtractsGrid.Items.Add(_item);

                    DefExtract.ExtractsGrid_AddItem(_item);
                }
            }

            // After gathering all episodes, extract could start
            DefExtract.AllEpisodesReadyToExtract = true;
        }

        private async void GoToSearch_Click(object sender, RoutedEventArgs e)
        {
            ContinueToSearch.IsEnabled = false;
            SearchTabItem.IsSelected = await Task.Run(() => AnimepaheExtractor.InitializePuppeteer());
            ContinueToSearch.IsEnabled = true;
        }

        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            { // Try to destroy everything
                AnimepaheExtractor.FinishPuppeteer();

                if (DefExtract != null && DefExtract.Downloader != null)
                {
                    DefExtract.Downloader.StopDownload();

                    if(DefExtract.Downloader.dThread != null)
                        DefExtract.Downloader.dThread.Abort();
                }
            }
            catch { }

            // Terminate process
            System.Environment.Exit(1);
        }
    }

    public class ExtractGridItem : INotifyPropertyChanged
    {
        public double EpisodeNumber { get; set; }
        public int Quality { get; set; }
        public string FanSub { get; set; }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public string Status { get; set; }

        private ExtractionStatus _statusEnum;
        public ExtractionStatus StatusEnum
        {
            get => _statusEnum;

            set
            {
                if (_statusEnum != value)
                {
                    switch (value)
                    {
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
        private void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ExtractionStatus
    {
        Null,
        Queued,
        Starting,
        Downloading,
        Completed,
        Error
    }
}