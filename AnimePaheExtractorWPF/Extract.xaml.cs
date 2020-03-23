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

namespace AnimePaheExtractorWPF {
    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>
    public partial class Extract : UserControl {
        ExtractorComponentModel extractorCM;
        public Extract() {
            InitializeComponent();

            extractorCM = new ExtractorComponentModel();
            this.DataContext = extractorCM;
        }

        #region SerieToDownload
        private Dictionary<string, string> _serieToDownload;
        public Dictionary<string, string> SerieToDownload {
            get {
                return _serieToDownload;
            }

            set {
                _serieToDownload = value;

                value.TryGetValue("title", out string _t);
                extractorCM.Title = _t;

                value.TryGetValue("id", out string _id);
                extractorCM.Id = _id;
            }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e, int from, int to) {
            MainWindow.IsSearchTabEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {

        }

    }
    public static partial class EpisodesReadyToExtract {
        public static IList<Links> MultiLinksClass;
        public class Links {
            private IList<EpisodeExtractLink> linkList;
            internal IList<EpisodeExtractLink> LinkList { get => linkList; set => linkList = value; }
        }
    }
}
