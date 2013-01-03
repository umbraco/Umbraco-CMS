using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Web.Strategies.DataTypes
{
    /// <summary>
    /// Before Save Content/Media subscriber that checks for Upload fields and updates related fields accordingly.
    /// </summary>
    /// <remarks>
    /// This is an intermediate fix for the legacy DataTypeUploadField and the FileHandlerData, so that properties
    /// are saved correctly when using the Upload field on a (legacy) Document or Media class.
    /// </remarks>
    public class LegacyUploadFieldWorkaround : IApplicationStartupHandler
    {
        public LegacyUploadFieldWorkaround()
        {
            global::umbraco.cms.businesslogic.media.Media.BeforeSave += MediaBeforeSave;
            global::umbraco.cms.businesslogic.web.Document.BeforeSave += DocumentBeforeSave;
        }

        void DocumentBeforeSave(global::umbraco.cms.businesslogic.web.Document sender, global::umbraco.cms.businesslogic.SaveEventArgs e)
        {
            if (UmbracoSettings.ImageAutoFillImageProperties != null)
            {
                var property = sender.GenericProperties.FirstOrDefault(x => x.PropertyType.DataTypeDefinition.DataType.Id == new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));
                if (property == null)
                    return;


                FillProperties(sender.Content, property);
            }
        }

        void MediaBeforeSave(global::umbraco.cms.businesslogic.media.Media sender, global::umbraco.cms.businesslogic.SaveEventArgs e)
        {
            if (UmbracoSettings.ImageAutoFillImageProperties != null)
            {
                var property = sender.GenericProperties.FirstOrDefault(x => x.PropertyType.DataTypeDefinition.DataType.Id == new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));
                if (property == null)
                    return;


                FillProperties(sender.MediaItem, property);
            }
        }

        private void FillProperties(IContentBase content, global::umbraco.cms.businesslogic.property.Property property)
        {
            XmlNode uploadFieldConfigNode = global::umbraco.UmbracoSettings.ImageAutoFillImageProperties.SelectSingleNode(string.Format("uploadField [@alias = \"{0}\"]", property.PropertyType.Alias));

            if (uploadFieldConfigNode != null)
            {
                var fileSystem = new PhysicalFileSystem("/");

                var path = string.IsNullOrEmpty(property.Value.ToString())
                    ? string.Empty
                    : System.Web.Hosting.HostingEnvironment.MapPath(property.Value.ToString());

                if (string.IsNullOrWhiteSpace(path) == false && fileSystem.FileExists(path))
                {
                    long size;
                    using (var fileStream = fileSystem.OpenFile(path))
                    {
                        size = fileStream.Length;
                    }

                    var extension = fileSystem.GetExtension(path) != null
                        ? fileSystem.GetExtension(path).Substring(1).ToLowerInvariant()
                        : "";

                    var supportsResizing = ("," + UmbracoSettings.ImageFileTypes + ",").Contains(string.Format(",{0},", extension));
                    var dimensions = supportsResizing ? GetDimensions(path) : null;

                    // only add dimensions to web images
                    UpdateProperty(uploadFieldConfigNode, content, "widthFieldAlias", supportsResizing ? dimensions.Item1.ToString(CultureInfo.InvariantCulture) : string.Empty);
                    UpdateProperty(uploadFieldConfigNode, content, "heightFieldAlias", supportsResizing ? dimensions.Item2.ToString(CultureInfo.InvariantCulture) : string.Empty);

                    UpdateProperty(uploadFieldConfigNode, content, "lengthFieldAlias", size == default(long) ? string.Empty : size.ToString(CultureInfo.InvariantCulture));
                    UpdateProperty(uploadFieldConfigNode, content, "extensionFieldAlias", string.IsNullOrEmpty(extension) ? string.Empty : extension);
                }
            }
        }

        private void UpdateProperty(XmlNode uploadFieldConfigNode, IContentBase content, string propertyAlias, object propertyValue)
        {
            XmlNode propertyNode = uploadFieldConfigNode.SelectSingleNode(propertyAlias);
            if (propertyNode != null && !String.IsNullOrEmpty(propertyNode.FirstChild.Value))
            {
                if (content.Properties.Contains(propertyNode.FirstChild.Value) && content.Properties[propertyNode.FirstChild.Value] != null)
                {
                    content.SetValue(propertyNode.FirstChild.Value, propertyValue);
                }
            }
        }

        private Tuple<int, int> GetDimensions(string path)
        {
            var fileSystem = new PhysicalFileSystem("/");
            
            int fileWidth;
            int fileHeight;
            using (var stream = fileSystem.OpenFile(path))
            {
                using (var image = Image.FromStream(stream))
                {
                    fileWidth = image.Width;
                    fileHeight = image.Height;
                }
            }

            return new Tuple<int, int>(fileWidth, fileHeight);
        }
    }
}