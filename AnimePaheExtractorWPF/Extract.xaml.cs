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
        public static Serie CurrentSerie;
        public static IList<Episode> CurrentEpisodes;

        ExtractorComponentModel extractorCM;

        public Extract() {
            InitializeComponent();

            extractorCM = new ExtractorComponentModel();
            this.DataContext = extractorCM;

            extractorCM.Title = CurrentSerie.Title;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            MainWindow.IsSearchTabEnabled = false;

        }
    }
}
