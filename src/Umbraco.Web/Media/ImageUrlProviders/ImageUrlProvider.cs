using System.Collections.Specialized;
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

        public override string GetImageUrlFromMedia(int mediaId, NameValueCollection parameters)
        {
            string url = string.Empty;
            var nodeIterator = library.GetMedia(mediaId, false);
            if (nodeIterator.Current != null)
            {
                var filename = getProperty(nodeIterator, "umbracoFile");
                string withThumb = addThumbInfo(filename, parameters);
                url = addCropInfo(withThumb, parameters);
            }

            return url;
        }

        public override string GetImageUrlFromFileName(string specifiedSrc, NameValueCollection parameters)
        {
            string withThumb = addThumbInfo(specifiedSrc, parameters);
            return addCropInfo(withThumb, parameters);
        }

        private string addThumbInfo(string filename, NameValueCollection parameters)
        {
            string thumb = string.Empty;
            if (!string.IsNullOrEmpty(parameters["thumb"]))
            {
                thumb = parameters["thumb"];
            }

            if(!string.IsNullOrEmpty(thumb))
            {
                int lastIndexOf = filename.LastIndexOf('.');
                string name = filename.Substring(0, lastIndexOf);
                string extension = filename.Substring(lastIndexOf, filename.Length - lastIndexOf);
                return string.Format("{0}_thumb_{1}{2}", name, thumb, extension);
            }
            return filename;
        }

        private string addCropInfo(string filename, NameValueCollection parameters)
        {
            string crop = string.Empty;
            if (!string.IsNullOrEmpty(parameters["crop"]))
            {
                crop = parameters["crop"];
            }

            if (!string.IsNullOrEmpty(crop))
            {
                int lastIndexOf = filename.LastIndexOf('.');
                string name = filename.Substring(0, lastIndexOf);
                string extension = filename.Substring(lastIndexOf, filename.Length - lastIndexOf);
                return string.Format("{0}_{1}{2}", name, crop, extension);
            }
            return filename;
        }


        private static string getProperty(XPathNodeIterator nodeIterator, string fileProp)
        {
            string xpath = UmbracoSettings.UseLegacyXmlSchema
                               ? string.Format(".//data[@alias = '{0}']", fileProp)
                               : string.Format(".//{0}", fileProp);
            var file = nodeIterator.Current.SelectSingleNode(xpath).InnerXml;
            return file;
        }
    }
}