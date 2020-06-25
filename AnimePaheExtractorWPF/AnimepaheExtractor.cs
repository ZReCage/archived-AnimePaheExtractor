using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using PuppeteerSharp;
using System;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace AnimePaheExtractorWPF
{
    class AnimepaheExtractor
    {
        static Browser DefaultBrowser = null;
        static Page CurrentPage;
        static bool InitializingPuppeteer;

        public static bool InitializePuppeteer()
        {
            if (!InitializingPuppeteer)
            {

                if (DefaultBrowser != null && DefaultBrowser.IsClosed)
                {
                    DefaultBrowser.Dispose();
                    DefaultBrowser = null;
                }

                if (DefaultBrowser == null)
                {
                    InitializingPuppeteer = true;

                    try
                    {
                        MainWindow.mainWindowCM.SetStatusBar(MainWindowComponentModel.StatusBarEnum.PreparingChromium);

                        RevisionInfo _revisionInfo = new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision).Result;

                        if (_revisionInfo.Downloaded)
                        {
                            MainWindow.mainWindowCM.SetStatusBar(MainWindowComponentModel.StatusBarEnum.Launching);

                            DefaultBrowser = Puppeteer.LaunchAsync(new LaunchOptions
                            {
                                Headless = false
                            }).Result;

                            IList<Page> _p = DefaultBrowser.PagesAsync().Result;
                            CurrentPage = _p[0];

                            MainWindow.mainWindowCM.SetStatusBar(MainWindowComponentModel.StatusBarEnum.Ready);
                            return true;
                        }
                        else
                        {
                            MainWindow.mainWindowCM.SetStatusBar(MainWindowComponentModel.StatusBarEnum.Error);
                            return false;
                        }
                    }
                    catch {
                        MainWindow.mainWindowCM.SetStatusBar(MainWindowComponentModel.StatusBarEnum.Error);
                    }

                    InitializingPuppeteer = false;
                }
            }
            return true;
        }

        public static void FinishPuppeteer()
        {
            if (DefaultBrowser != null)
            {
                DefaultBrowser.Dispose();
                DefaultBrowser = null;
            }
        }

        public static async Task<string> GetUrlToExtract(string serverUrl)
        {
            string _urlExtracted = null;

            Task<Response> gotoPageToLoad = CurrentPage.GoToAsync(serverUrl);

            await CurrentPage.SetRequestInterceptionAsync(true);

            EventHandler<RequestEventArgs> _request = null;
            _request = async (s, e) =>
            {
                if (_urlExtracted != null)
                {
                    try
                    {
                        await e.Request.AbortAsync();
                        CurrentPage.Request -= _request;
                        await CurrentPage.GoToAsync("about:blank");
                    }
                    catch
                    {
                        // Request is already handled
                    }

                }
                else
                    try
                    {
                        await e.Request.ContinueAsync();
                    }
                    catch
                    {
                        // It may continue without any trouble
                    }
            };
            CurrentPage.Request += _request;

            EventHandler<ResponseCreatedEventArgs> _response = null;
            _response = (s, e) =>
            {
                if (e.Response.Url.Contains("https://kwik.cx/d/"))
                {
                    if (e.Response.Headers.TryGetValue("location", out _urlExtracted))
                    {
                        CurrentPage.Response -= _response;
                    }
                }

            };
            CurrentPage.Response += _response;

            await gotoPageToLoad;

            await CurrentPage.WaitForSelectorAsync("button");
            await CurrentPage.ClickAsync("button");

            return _urlExtracted;
        }

        public static async Task<SearchResults> Search(string query)
        {
            query = query.Length > 0 ? query : "boku no piko"; // ???

            string uri = "https://animepahe.com/api?m=search&l=8&q=" + query.Substring(0, query.Length > 32 ? 32 : query.Length);

            string _json = await GetJSON(uri);
            //string _json = await GetRequest(uri);

            // Checks _json
            if (_json.Length > 0)
                return JsonConvert.DeserializeObject<SearchResults>(_json);
            else
                return new SearchResults();
        }

        public static async Task<IList<Episode>> GetEpisodesList(int _serieId, Range _range)
        {
            int actualPage = _range != null ? _range.From / 31 + 1 : 1;
            int? lastPage = null;

            IList<Episode> _episodes = new List<Episode>();

            do
            {
                string uri = "https://animepahe.com/" + $"api?m=release&id={_serieId}&sort=episode_asc" + $"&page={actualPage}";

                string _request = await GetJSON(uri);

                JObject jsonObject = JObject.Parse(_request);
                if (lastPage == null)
                    lastPage = Convert.ToInt32(jsonObject.SelectToken("last_page").ToString());

                foreach (JToken fundingSource in jsonObject.SelectTokens("data[*]"))
                {
                    double _episodeNumber = Convert.ToDouble(fundingSource.SelectToken("episode").ToString());
                    if (_range != null && _episodeNumber < _range.From)
                        // If the episode number is lesser than "from" then continue
                        continue;
                    string _session = fundingSource.SelectToken("session").ToString();

                    _episodes.Add(new Episode(_episodeNumber, _session));

                    if (_range != null && _episodeNumber >= _range.To)
                        // If the episode number is greater than "to" or equal, return
                        return _episodes;
                }

                actualPage++;
            } while (actualPage <= lastPage);

            return _episodes;
        }

        public static async Task<IList<EpisodeLinkData>> GetEpisodeLinksData(int serieId, string session)
        {
            // Request
            string uri = "https://animepahe.com/api?m=embed&p=kwik" + $"&id={serieId}" + $"&session={session}";
            string _request = await GetJSON(uri);

            try
            {
                JObject jsonObject = JObject.Parse(_request);
                IList<EpisodeLinkData> episodeExtractLink = new List<EpisodeLinkData>();

                // Selects only 720 or poorer I'm sorry little ones
                JToken fundingSource = jsonObject.SelectTokens("$.*[0]").First();
                
                EpisodeLinkData _e = new EpisodeLinkData();

                // Gets quality, most of the time it is 720p
                _e.Quality = Convert.ToInt32(fundingSource.Children<JProperty>().First().Name);

                // FanSub credits
                _e.FanSub = fundingSource.SelectToken("$.*.fansub").ToString();

                // Asks for the episode player url, I use a .Replace to get the episode download url
                // They changed from url to kwik recently
                string url = fundingSource.SelectToken("$.*.kwik").ToString().Replace("/e/", "/f/");
                _e.Url = url;

                episodeExtractLink.Add(_e);

                return episodeExtractLink;

            }
            catch
            {
                return new List<EpisodeLinkData>();
            }
        }

        public static async Task<string> GetJSON(string uri)
        {
            // Makes sure Puppeteer is OK
            InitializePuppeteer();

            Page _tmpPage = await DefaultBrowser.NewPageAsync();
            Response response = await _tmpPage.GoToAsync(uri);
            // This block waits until there's no title, a sign that the json was loaded
            string title = string.Empty;
            do
            {
                Thread.Sleep(500);
                try
                {
                    title = await _tmpPage.GetTitleAsync();
                }
                catch (EvaluationFailedException e)
                {
#if DEBUG
                    Debug.WriteLine(e.Message);
#endif
                    response = await _tmpPage.ReloadAsync();
                }
            } while (title != string.Empty);

            // Get response's buffer, encodes as string
            byte[] buffer = await response.BufferAsync();

            // Temp page closes
            await _tmpPage.CloseAsync();

            return Encoding.UTF8.GetString(buffer);
        }

        public static async Task<BitmapImage> GetImage(string uri)
        {
            // Makes sure Puppeteer is OK
            InitializePuppeteer();

            Page _tmpPage = await DefaultBrowser.NewPageAsync();
            Response response = await _tmpPage.GoToAsync(uri);

            // The image title should contain one of these
            IList<string> fileTypes = new List<string> { ".jpg", ".png", ".bmp", ".gif" };

            // This block waits until the title matches the condition
            string title = string.Empty;
            do
            {
                Thread.Sleep(500);
                try
                {
                    title = await _tmpPage.GetTitleAsync();
                }
                catch (EvaluationFailedException e)
                {
#if DEBUG
                    Debug.WriteLine(e.Message);
#endif
                    response = await _tmpPage.ReloadAsync();
                }
                catch (NullReferenceException e)
                {
#if DEBUG
                    Debug.WriteLine(e.Message);
#endif
                    // F
                }
            } while (!fileTypes.Any(s => title.Contains(s)));

            // Gets response's buffer, encodes as bitmap (No tested yet)
            byte[] buffer = await response.BufferAsync();
            BitmapImage _bitmap = new BitmapImage();
            _bitmap.BeginInit();
            using (var mStream = new MemoryStream(buffer))
            {
                _bitmap.StreamSource = mStream;
            }
            _bitmap.EndInit();

            // Temp page closes
            await _tmpPage.CloseAsync();

            return _bitmap;
        }

        public static string GetRequest(string uri, int timeoutSecs = 10)
        {
            try { 
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Timeout = timeoutSecs * 1000;

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                // Exception occurred
                return "";
            }
        }
    }
    class SearchResults
    {
        public int Total { get; set; }
        public IList<Dictionary<string, string>> Data { get; set; }
    }

    public class Serie
    {
        public string Title;
        public int Id;

        public Serie(string _title, int _id)
        {
            Title = _title;
            Id = _id;
        }
    }

    public class Range
    {
        public int From;
        public int To;
    }

    public class Episode
    {
        public double EpisodeNumber;
        public string Session;
        public IList<EpisodeLinkData> EpisodeLinksData;

        public Episode(double episodeNumber, string session)
        {
            EpisodeNumber = episodeNumber;
            Session = session;
        }

        public async Task<bool> GatherEpisodeLinksData(int serieId)
        {
            EpisodeLinksData = await AnimepaheExtractor.GetEpisodeLinksData(serieId, Session);
            if (EpisodeLinksData != null)
            {
                return true;
            }
            else return false;
        }

    }

    public class EpisodeLinkData
    {
        public int Quality;
        public string FanSub;
        public string Url;
    }
}
