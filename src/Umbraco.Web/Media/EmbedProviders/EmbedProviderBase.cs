using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Xml;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.EmbedProviders
{
    public abstract class EmbedProviderBase : IEmbedProvider
    {
        private static HttpClient _httpClient;

        public abstract string ApiEndpoint { get; }

        public abstract string[] UrlSchemeRegex { get; }

        public abstract Dictionary<string, string> RequestParams { get; }
        
        public abstract string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0);

        public virtual string GetEmbedProviderUrl(string url, int maxWidth, int maxHeight)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) == false)
                throw new ArgumentException("Not a valid URL.", nameof(url));

            var fullUrl = new StringBuilder();

            fullUrl.Append(ApiEndpoint);
            fullUrl.Append("?url=" + HttpUtility.UrlEncode(url));

            foreach (var param in RequestParams)
                fullUrl.Append($"&{param.Key}={param.Value}");

            if (maxWidth > 0)
                fullUrl.Append("&maxwidth=" + maxWidth);

            if (maxHeight > 0)
                fullUrl.Append("&maxheight=" + maxHeight);

            return fullUrl.ToString();
        }
        
        public virtual string DownloadResponse(string url)
        {
            if (_httpClient == null)
                _httpClient = new HttpClient();

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                var response = _httpClient.SendAsync(request).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public virtual T GetJsonResponse<T>(string url) where T : class
        {
            var response = DownloadResponse(url);
            return JsonConvert.DeserializeObject<T>(response);
        }

        public virtual XmlDocument GetXmlResponse(string url)
        {
            var response = DownloadResponse(url);
            var doc = new XmlDocument();
            doc.LoadXml(response);

            return doc;
        }

        public virtual string GetXmlProperty(XmlDocument doc, string property)
        {
            var selectSingleNode = doc.SelectSingleNode(property);
            return selectSingleNode != null ? selectSingleNode.InnerText : string.Empty;
        }
    }
};
