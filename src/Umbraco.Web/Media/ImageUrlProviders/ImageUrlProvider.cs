using System.Collections.Generic;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
using Umbraco.Core.Media;
using umbraco;
using Umbraco.Core;

namespace Umbraco.Web.Media.ImageUrlProviders
{
    public class ImageUrlProvider : IImageUrlProvider
    {
        public const string DefaultName = "umbracoUpload";

        public string Name
        {
            get { return DefaultName; }
        }

        public string GetImageUrlFromMedia(int mediaId, IDictionary<string, string> parameters)
        {
            var url = string.Empty;

            var nodeIterator = library.GetMedia(mediaId, false);

            if (nodeIterator.Current != null)
            {
                var filename = GetProperty(nodeIterator, Constants.Conventions.Media.File);
                var withThumb = AddThumbInfo(filename, parameters);
                url = AddCropInfo(withThumb, parameters);
            }

            return url;
        }

        public string GetImageUrlFromFileName(string specifiedSrc, IDictionary<string, string> parameters)
        {
            var withThumb = AddThumbInfo(specifiedSrc, parameters);
            return AddCropInfo(withThumb, parameters);
        }

        private static string AddThumbInfo(string filename, IDictionary<string, string> parameters)
        {
            var thumb = string.Empty;
            if (parameters.ContainsKey("thumb"))
                thumb = parameters["thumb"];

            if (!string.IsNullOrEmpty(thumb) && filename.Contains("."))
            {
                var lastIndexOf = filename.LastIndexOf('.');
                var name = filename.Substring(0, lastIndexOf);
                var extension = filename.Substring(lastIndexOf, filename.Length - lastIndexOf);
                return string.Format("{0}_thumb_{1}{2}", name, thumb, extension);
            }
            return filename;
        }

        private static string AddCropInfo(string filename, IDictionary<string, string> parameters)
        {
            var crop = string.Empty;
            if (parameters.ContainsKey("crop"))
                crop = parameters["crop"];

            if (!string.IsNullOrEmpty(crop) && filename.Contains("."))
            {
                var lastIndexOf = filename.LastIndexOf('.');
                var name = filename.Substring(0, lastIndexOf);
                
                //var extension = filename.Substring(lastIndexOf, filename.Length - lastIndexOf);
                //Built in cropper currently always uses jpg as an extension

                const string extension = ".jpg";
                return string.Format("{0}_{1}{2}", name, crop, extension);
            }

            return filename;
        }


        private static string GetProperty(XPathNodeIterator nodeIterator, string fileProp)
        {
            var xpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema
                               ? string.Format(".//data[@alias = '{0}']", fileProp)
                               : string.Format(".//{0}", fileProp);

            var file = string.Empty;
            var selectSingleNode = nodeIterator.Current.SelectSingleNode(xpath);
            if (selectSingleNode != null)
                file = selectSingleNode.InnerXml;

            return file;
        }
    }
}