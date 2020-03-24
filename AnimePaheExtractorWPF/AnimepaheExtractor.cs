using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using PuppeteerSharp;
using System;
using Newtonsoft.Json.Linq;

namespace AnimePaheExtractorWPF {
    class AnimepaheExtractor {
        static Browser DefaultBrowser = null;
        static Page CurrentPage;
        static string UrlExtracted;

        static WebClient WC;

        public static async Task<bool> InitializePuppeteer() {
            if(DefaultBrowser == null) {
                try {
                    WC = new WebClient();

                    await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                    DefaultBrowser = await Puppeteer.LaunchAsync(new LaunchOptions {
                        Headless = false
                    });
                    IList<Page> _p = await DefaultBrowser.PagesAsync();
                    CurrentPage = _p[0];

                    await CurrentPage.SetRequestInterceptionAsync(true);
                    CurrentPage.Request += Request;
                    CurrentPage.Response += Response;

                    return true;
                } catch {
                    return false;
                }
            }
            return true;
        }

        public static async void GetUrlToExtract(string serverUrl) {
            Task<Response> gotoPageToLoad = CurrentPage.GoToAsync(serverUrl);

            await gotoPageToLoad;

            await CurrentPage.WaitForSelectorAsync("button");
            await CurrentPage.ClickAsync("button");
        }

        static async void Request(object sender, RequestEventArgs e) {
            // if pageToLoad
            if (e.Request.IsNavigationRequest && e.Request.Url.Contains("https://kwik.cx/f/")) {
                // Adds referer, needed to load page for some reason
                e.Request.Headers.Add("referer", "https://kwik.cx");

                Payload data = new Payload {
                    Headers = e.Request.Headers,
                };

                await e.Request.ContinueAsync(data);

            } else if (UrlExtracted != null) {
                await e.Request.AbortAsync();

                CurrentPage.Request -= Request;

            } else
                await e.Request.ContinueAsync();
        }
        static void Response(object sender, ResponseCreatedEventArgs e) {
            if (e.Response.Url.Contains("https://kwik.cx/d/")) {
                // Download link
                e.Response.Headers.TryGetValue("location", out UrlExtracted);
            }
        }

        public static async Task<SearchResults> Search(string query) {
            query = query.Length > 0 ? query : "boku no piko";

            string uri = "https://animepahe.com/api?m=search&l=8&q=" + query.Substring(0, query.Length > 32 ? 32 : query.Length);

            string _json = await GetRequest(uri);
            return JsonConvert.DeserializeObject<SearchResults>(_json);
        }

        public static async Task<IList<Episode>> GetEpisodesList(int _serieId, Range _range) {
            int actualPage = _range != null ? _range.From / 31 + 1 : 1;
            int? lastPage = null;

            IList<Episode> _episodes = new List<Episode>();

            do {
                string uri = "https://animepahe.com/" + $"api?m=release&id={_serieId}&sort=episode_asc" + $"&page={actualPage}";

                string _request = await GetRequest(uri);

                JObject jsonObject = JObject.Parse(_request);
                if(lastPage == null)
                    lastPage = Convert.ToInt32(jsonObject.SelectToken("last_page").ToString());

                foreach (JToken fundingSource in jsonObject.SelectTokens("data[*]")) {
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

        public static async Task<IList<EpisodeLinkData>> GetEpisodeLinksData(int serieId, string session) {
            // Request
            string uri = "https://animepahe.com/api?m=embed&p=kwik" + $"&id={serieId}" + $"&session={session}";
            string _request = await GetRequest(uri);

            try {
                JObject jsonObject = JObject.Parse(_request);
                IList<EpisodeLinkData> episodeExtractLink = new List<EpisodeLinkData>();

                foreach (JToken fundingSource in jsonObject.SelectTokens("$.*.*")) {
                    EpisodeLinkData _e = new EpisodeLinkData();
                    string quality;

                    foreach (JProperty _jProperty in fundingSource.Children<JProperty>()) {
                        quality = _jProperty.Name;
                        _e.Quality = Convert.ToInt32(quality);
                    }

                    string fansub = fundingSource.SelectToken("$.*.fansub").ToString();
                    _e.FanSub = fansub;

                    string url = fundingSource.SelectToken("$.*.url").ToString().Replace("/e/", "/f/");
                    _e.Url = url;

                    episodeExtractLink.Add(_e);
                }

                return episodeExtractLink;

            } catch {
                return new List<EpisodeLinkData>();
            }
        }

        public static async Task<string> GetRequest(string uri) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                return await reader.ReadToEndAsync();
            }
        }
    }
    class SearchResults {
        public int Total { get; set; }
        public IList<Dictionary<string, string>> Data { get; set; }
    }

    public class Serie {
        public string Title;
        public int Id;

        public Serie(string _title, int _id) {
            Title = _title;
            Id = _id;
        }
    }

    public class Range {
        public int From;
        public int To;
    }

    public class Episode {
        public double EpisodeNumber;
        public string Session;
        public IList<EpisodeLinkData> EpisodeLinksData;

        public Episode(double episodeNumber, string session) {
            EpisodeNumber = episodeNumber;
            Session = session;
        }

        public async Task<bool> GatherEpisodeLinksData(int serieId) {
            EpisodeLinksData = await AnimepaheExtractor.GetEpisodeLinksData(serieId, Session);
            if (EpisodeLinksData != null) {
                return true;
            } else return false;
        }

    }

    public class EpisodeLinkData {
        public int Quality;
        public string FanSub;
        public string Url;
    }
}
