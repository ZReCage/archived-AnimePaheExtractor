using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace AnimePaheExtractorWPF {
    class AnimepaheExtractor {
        public static string SearchResults;

        public static async void Search(string query) {
            query = query.Length > 0 ? query : "overlord";

            string url = "https://animepahe.com/api?m=search&l=8&q=" + query.Substring(0, query.Length>32 ? 32 : query.Length);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                 = await reader.ReadToEndAsync();

                JsonSerializer.Deserialize
            }
        }

    }
}
