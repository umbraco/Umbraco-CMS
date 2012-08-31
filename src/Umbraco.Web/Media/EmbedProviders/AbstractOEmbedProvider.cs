using System;
using Umbraco.Core.Embed;
using System.Text;
using System.Xml;
using System.Collections.Generic;

namespace Umbraco.Web.Media.EmbedProviders
{
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

        //public virtual string GetPreview(string url, int maxWidth, int maxHeight)
        //{
        //    return GetMarkup(url, maxWidth, maxHeight);
        //}

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

        public virtual XmlDocument GetXmlResponse(string url)
        {
            var webClient = new System.Net.WebClient();

            var response = webClient.DownloadString(url);

            var doc = new XmlDocument();
            doc.LoadXml(response);

            return doc;
        }
    }
}