using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.EmbedProviders
{
    //TODO: Make all Http calls async

    public abstract class AbstractOEmbedProvider: IEmbedProvider
    {
        public virtual bool SupportsDimensions
        {
            get { return true; }
        }

        [ProviderSetting]
        public string APIEndpoint{ get;set; }

        [ProviderSetting]
        public Dictionary<string, string> RequestParams{ get;set; }

        public abstract string GetMarkup(string url, int maxWidth, int maxHeight);

        public virtual string BuildFullUrl(string url, int maxWidth, int maxHeight)
        {
            var fullUrl = new StringBuilder();

            fullUrl.Append(APIEndpoint);
            fullUrl.Append("?url=" + url);

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
            using (var webClient = new WebClient())
            {
                return webClient.DownloadString(url);
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