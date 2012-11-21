using System.Xml.XPath;
using Umbraco.Core.Media;
using umbraco;

namespace Umbraco.Web.Media.ImageUrlProviders
{
    public class ImageUrlProvider : ImageUrlProviderBase
    {
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            //Get default values from the provider config here?
            base.Initialize(name, config);
        }

        public override string GetImageUrlFromMedia(int mediaId)
        {
            string url = string.Empty;
            var nodeIterator = library.GetMedia(mediaId, false);
            if (nodeIterator.Current != null)
            {
                var filename = getProperty(nodeIterator, "umbracoFile");
                var fileExtension = getProperty(nodeIterator, "umbracoExtension");
                url = filename;
            }

            return url;
        }

        private static string getProperty(XPathNodeIterator nodeIterator, string fileProp)
        {
            string xpath = UmbracoSettings.UseLegacyXmlSchema
                               ? string.Format(".//data[@alias = '{0}']", fileProp)
                               : string.Format(".//{0}", fileProp);
            var file = nodeIterator.Current.SelectSingleNode(xpath).InnerXml;
            return file;
        }

        public override string GetImageUrlFromFileName(string specifiedSrc)
        {
            return specifiedSrc + "?wheee=1234";
        }
    }
}