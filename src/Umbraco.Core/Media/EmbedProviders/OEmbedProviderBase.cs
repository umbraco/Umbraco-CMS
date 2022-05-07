using System.Globalization;
using System.Net;
using System.Xml;
using Microsoft.AspNetCore.WebUtilities;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders
{
    public abstract class OEmbedProviderBase : IEmbedProvider
    {
        private readonly IJsonSerializer _jsonSerializer;

        protected OEmbedProviderBase(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        private static HttpClient? _httpClient;

        public abstract string ApiEndpoint { get; }

        public abstract string[] UrlSchemeRegex { get; }

        public abstract Dictionary<string, string> RequestParams { get; }

        public abstract string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0);

        public virtual string GetEmbedProviderUrl(string url, int maxWidth, int maxHeight)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) == false)
            {
                throw new ArgumentException("Not a valid URL.", nameof(url));
            }

            var queryString = new Dictionary<string, string?>
            {
                { "url", url }
            };

            foreach (var param in RequestParams)
            {
                queryString.Add(param.Key, param.Value);
            }

            if (maxWidth > 0)
            {
                queryString.Add("maxwidth", maxWidth.ToString(CultureInfo.InvariantCulture));
            }

            if (maxHeight > 0)
            {
                queryString.Add("maxheight", maxHeight.ToString(CultureInfo.InvariantCulture));
            }

            return QueryHelpers.AddQueryString(ApiEndpoint, queryString);
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

        public virtual T? GetJsonResponse<T>(string url) where T : class
        {
            var response = DownloadResponse(url);
            return _jsonSerializer.Deserialize<T>(response);
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
