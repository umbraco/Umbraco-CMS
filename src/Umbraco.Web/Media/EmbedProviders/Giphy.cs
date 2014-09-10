using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class Giphy : AbstractProvider
    {
        public override string GetMarkup(string url, int maxWidth, int maxHeight)
        {
            var u = new Uri(url);
            var id = u.Segments.Last().Split('-').Last();
            var api = string.Format("http://api.giphy.com/v1/gifs/{0}?api_key=dc6zaTOxFJmzC", id);

            var apiClient = new HttpClient();

            var dataFromAPI = apiClient.GetAsync(api).Result;

            var result = new JObject();

            if (dataFromAPI.IsSuccessStatusCode)
            {
                var APIresult = dataFromAPI.Content.ReadAsStringAsync();

                result = JObject.Parse(APIresult.Result);

                var embedUrl = result.SelectToken("data.images.original.url").Value<string>();

                return string.Format("<img src=\"{0}\"/>", embedUrl);
            }

            return string.Format("<iframe src=\"//giphy.com/embed/{0}\" width=\"{1}\" height=\"{2}\" frameBorder=\"0\" webkitAllowFullScreen mozallowfullscreen allowFullScreen></iframe>",
              "bJAi9R0WWOohO", 250, 153);
        }
    }
}
