using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AnimePaheExtractorWPF {
    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>

    public partial class Extract : UserControl {
        // Do not do this
        public static Serie CurrentSerie;
        ExtractGridItem CurrentGridItem = null;
        bool isExtractionStarted = false;

        public ExtractComponentModel ExtractCM;

        public Downloader Downloader;

        public Extract() {
            InitializeComponent();
            StartExtraction_SetWhenEnableable();

            ExtractCM = new ExtractComponentModel();
            this.DataContext = ExtractCM;

            ExtractCM.Title = CurrentSerie.Title;
        }

        private void StartExtraction_Click(object sender, RoutedEventArgs e) {
            StartExtraction.IsEnabled = false;
            isExtractionStarted = true;
            ExtractStart();
        }

        private async void ExtractStart() {
            Thread.Sleep(1500);

            if (CurrentGridItem != null) {
                Downloader Downloader = new Downloader();
                Downloader.ProgressChanged += _downloader_ProgressChanged;
                Downloader.Completed += _downloader_Completed;

                CurrentGridItem.StatusEnum = ExtractionStatus.Starting;

                // Sets Serie path
                string _title = CurrentSerie.Title;
                string _epNumber = CurrentGridItem.Episode.EpisodeNumber.ToString();


                foreach (var _c in Path.GetInvalidFileNameChars()) {
                    _title = _title.Replace(_c, '-');
                }
                
                foreach (var _c in Path.GetInvalidFileNameChars()) {
                    _epNumber = _epNumber.Replace(_c, '-');
                }

                DirectoryInfo _directory = Directory.CreateDirectory($"{ Directory.GetCurrentDirectory()}\\{_title}");
                string _fileName = $"{_directory.FullName}\\Episode {_epNumber}.mp4";

                // File already exists, then ERROR
                if (File.Exists(_fileName)) {
                    CurrentGridItem.StatusEnum = ExtractionStatus.Error;
                    // Next file
                    SetNextFile();
                
                } else { // Continue if file doesn't exist
                    // Get url
                    string _urlToExtract = null;

                    while (_urlToExtract == null) {
                        try {
                            AnimepaheExtractor.InitializePuppeteer(this); // Try to bring back to life the browser
                            _urlToExtract = await AnimepaheExtractor.GetUrlToExtract(CurrentGridItem.Episode.EpisodeLinksData[0].Url);
                        } catch {
                            Thread.Sleep(500);
                        }
                
                    }

                    // Starts Download
                    new Thread(new ThreadStart(() => { Downloader.DownloadFile(_urlToExtract, _fileName); })).Start();
                    CurrentGridItem.StatusEnum = ExtractionStatus.Downloading;
                }
            }
        }

        public void ExtractsGrid_AddItem(ExtractGridItem item) { // Check its behavior, there are better approaches
            if(CurrentGridItem == null) {
                CurrentGridItem = item;

                if (isExtractionStarted) {
                    ExtractStart();
                }
            }
        }

        public bool IsEnableable = false;
        public async void StartExtraction_SetWhenEnableable() {
            await Task.Factory.StartNew(() => { while (!IsEnableable); } );
            StartExtraction.IsEnabled = true;
        }

        private void _downloader_Completed(object sender, EventArgs e) {
            CurrentGridItem.StatusEnum = ExtractionStatus.Completed;

            // Next file
            SetNextFile();
        }

        private void SetNextFile() {
            int _idx = ExtractsGrid.Items.IndexOf(CurrentGridItem) + 1;
            if (_idx < ExtractsGrid.Items.Count) {
                CurrentGridItem = (ExtractGridItem)ExtractsGrid.Items[_idx];

                ExtractStart();
            } else
                CurrentGridItem = null;
        }

        private void _downloader_ProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            CurrentGridItem.Progress = (int)e.ProgressPercentage;
        }

        /*private void wcDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            CurrentGridItem.Progress = (int)e.ProgressPercentage;
        }*/

        /*void wcDownloadFileCompleted(object sender, EventArgs e) {
            CurrentGridItem.Status = ExtractionStatus.Completed;
            ((WebClient)sender).Dispose();

            // Next file
            try {
                CurrentGridItem = (ExtractGridItem)ExtractsGrid.Items[ExtractsGrid.Items.IndexOf(CurrentGridItem) + 1];
            } catch { CurrentGridItem = null; }

            ExtractStart();
        }*/
    }

    // Workaround non-completed downloads
    public class Downloader {
        public event EventHandler<DownloadStatusChangedEventArgs> ResumablityChanged;
        public event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged;
        public event EventHandler Completed;

        public bool stop = true; // by default stop is true

        public void DownloadFile(string downloadLink, string path) {
            stop = false; // always set this bool to false, everytime this method is called

            var fileInfo = new FileInfo(path);
            long existingLength = 0;
            if (fileInfo.Exists)
                existingLength = fileInfo.Length;

            var request = (HttpWebRequest)HttpWebRequest.Create(downloadLink);
            request.Proxy = null;
            request.AddRange(existingLength);

            try {
                using (var response = (HttpWebResponse)request.GetResponse()) {
                    long fileSize = existingLength + response.ContentLength; //response.ContentLength gives me the size that is remaining to be downloaded
                    bool downloadResumable; // need it for sending empty progress

                    if (response.StatusCode == HttpStatusCode.PartialContent) {
                        downloadResumable = true;
                    } else // sometimes a server that supports partial content will lose its ability to send partial content(weird behavior) and thus the download will lose its resumability
                      {
                        // BAD NEWS _logger.Log("Resume Not Supported");
                        existingLength = 0;
                        downloadResumable = false;
                    }
                    OnResumabilityChanged(new DownloadStatusChangedEventArgs(downloadResumable));

                    using (var saveFileStream = fileInfo.Open(downloadResumable ? FileMode.Append : FileMode.Create, FileAccess.Write))
                    using (var stream = response.GetResponseStream()) {
                        byte[] downBuffer = new byte[4096];
                        int byteSize = 0;
                        long totalReceived = byteSize + existingLength;
                        var sw = Stopwatch.StartNew();
                        _try:
                        try {
                            while (!stop && (byteSize = stream.Read(downBuffer, 0, downBuffer.Length)) > 0) {
                                saveFileStream.Write(downBuffer, 0, byteSize);
                                totalReceived += byteSize;

                                var currentSpeed = totalReceived / sw.Elapsed.TotalSeconds;
                                OnProgressChanged(new DownloadProgressChangedEventArgs(totalReceived, fileSize, (long)currentSpeed));
                            } 
                        } catch { goto _try; }
                        sw.Stop();
                    }
                }
                if (!stop)
                    OnCompleted(EventArgs.Empty);
            } catch /*(WebException e)*/ {
                // -- WEB EXCEPTION _logger.Log(e);
            }
        }

        public void StopDownload() {
            stop = true;
        }

        protected virtual void OnResumabilityChanged(DownloadStatusChangedEventArgs e) {
            var handler = ResumablityChanged;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnProgressChanged(DownloadProgressChangedEventArgs e) {
            var handler = ProgressChanged;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnCompleted(EventArgs e) {
            var handler = Completed;
            if (handler != null) {
                handler(this, e);
            }
        }
    }

    public class DownloadStatusChangedEventArgs : EventArgs {
        public DownloadStatusChangedEventArgs(bool canResume) {
            ResumeSupported = canResume;
        }
        public bool ResumeSupported { get; private set; }
    }

    public class DownloadProgressChangedEventArgs : EventArgs {
        public DownloadProgressChangedEventArgs(long totalReceived, long fileSize, long currentSpeed) {
            BytesReceived = totalReceived;
            TotalBytesToReceive = fileSize;
            CurrentSpeed = currentSpeed;
        }
        public long BytesReceived { get; private set; }
        public long TotalBytesToReceive { get; private set; }
        public float ProgressPercentage { get { return ((float)BytesReceived / (float)TotalBytesToReceive) * 100; } }
        /// <summary>in Bytes</summary>
        public long CurrentSpeed { get; private set; }
        public TimeSpan TimeLeft {
            get {
                var bytesRemainingtoBeReceived = TotalBytesToReceive - BytesReceived;
                return TimeSpan.FromSeconds(bytesRemainingtoBeReceived / CurrentSpeed);
            }
        }
    }
}
