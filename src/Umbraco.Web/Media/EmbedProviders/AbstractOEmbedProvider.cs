using System;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.EmbedProviders
{
    // TODO: Make all Http calls async

    public abstract class AbstractOEmbedProvider : IEmbedProvider
    {
        private static HttpClient _httpClient;

        public virtual bool SupportsDimensions
        {
            get { return true; }
        }

        [ProviderSetting]
        public string APIEndpoint { get; set; }

        [ProviderSetting]
        public Dictionary<string, string> RequestParams { get; set; }

        public abstract string GetMarkup(string url, int maxWidth, int maxHeight);

        public virtual string BuildFullUrl(string url, int maxWidth, int maxHeight)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) == false)
                throw new ArgumentException("Not a valid Url");

            var fullUrl = new StringBuilder();

            fullUrl.Append(APIEndpoint);
            fullUrl.Append("?url=" + HttpUtility.UrlEncode(url));

            foreach (var p in RequestParams)
                fullUrl.Append(string.Format("&{0}={1}", p.Key, p.Value));

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
}
