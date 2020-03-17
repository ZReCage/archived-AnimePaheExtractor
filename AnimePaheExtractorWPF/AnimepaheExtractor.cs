using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AnimePaheExtractorWPF {
    class AnimepaheExtractor {
        //public static string SearchResults;

        public static async Task<SearchResults> Search(string query) {
            query = query.Length > 0 ? query : "overlord";

            string url = "https://animepahe.com/api?m=search&l=8&q=" + query.Substring(0, query.Length > 32 ? 32 : query.Length);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream)) {
                    string _json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<SearchResults>(_json);
                }
        }
    }
    class SearchResults {
        public int Total { get; set; }
        public IList<Dictionary<string, string>> Data { get; set; }
    }
}
