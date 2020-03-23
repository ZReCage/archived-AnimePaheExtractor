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

        public static async Task<IList<string>> GetEpisodesList(int serieId, int from, int to) {
            // First, get "session" parameter, this is the episode ID
            int actualPage = 1;
            string uri = "https://animepahe.com/" + $"api?m=release&id={serieId}&sort=episode_asc" + $"&page={actualPage}";

            string _request = await GetRequest(uri);
            EpisodesList _eDeserialized = JsonConvert.DeserializeObject<EpisodesList>(_request);

            IList<string> _session = new List<string>();

            foreach (EpisodesList.EpisodeData data in _eDeserialized.Data)
                _session.Add(data.Session);

            // Not done yet
            // WC.DownloadFileAsync();

            return _session;
        }

        public static async Task<IList<EpisodeExtractLink>> GetEpisodeExtractLink(int serieId, string session) {
            // Request
            string uri = "https://animepahe.com/api?m=embed&p=kwik" + $"&id={serieId}" + $"&session={session}";
            string _request = await GetRequest(uri);

            try {
                JObject jsonObject = JObject.Parse(_request);
                IList<EpisodeExtractLink> episodeExtractLink = new List<EpisodeExtractLink>();

                foreach (JToken fundingSource in jsonObject.SelectTokens("$.*.*")) {
                    EpisodeExtractLink _e = new EpisodeExtractLink();
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
                return new List<EpisodeExtractLink>();
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

    class EpisodeExtractLink {
        public int Quality;
        public string FanSub;
        public string Url;
    }

    class EpisodesList {
        public int Total;
        public int Perpage;
        public int CurrentPage;
        public string NextPageUrl;
        public string PrevPageUrl;
        public int From;
        public int To;
        public IList<EpisodeData> Data;
        public class EpisodeData {
            public int Id;
            public int Anime_id;
            public int Episode;
            public int Episode2;
            public string Edition;
            public string Title;
            public string Snapshot;
            public string Disc;
            public string Duration;
            public string Session;
            public int Filler;
            public string CreatedAt;
        }
    }
}
