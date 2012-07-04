using System;
using System.IO;
using System.Web;
using System.Xml;
using umbraco.cms.businesslogic.Files;
using umbraco.cms.businesslogic.property;
using umbraco.IO;

namespace umbraco.cms.businesslogic.datatype
{
    public class FileHandlerData : DefaultData
    {
        private readonly string _thumbnailSizes;

        public FileHandlerData(BaseDataType DataType, string thumbnailSizes)
            : base(DataType)
        {
            _thumbnailSizes = thumbnailSizes;
        }

        public override object Value
        {
            get { return base.Value; }
            set
            {
                UmbracoFile um = null;
                if (value is HttpPostedFile)
                {
                    // handle upload
                    var file = value as HttpPostedFile;
                    if (file.FileName != String.Empty)
                    {
                        string fileName = UmbracoSettings.UploadAllowDirectories
                                              ? Path.Combine(PropertyId.ToString(), file.FileName)
                                              : PropertyId + "-" + file.FileName;

                        fileName = Path.Combine(SystemDirectories.Media, fileName);
                        um = UmbracoFile.Save(file, fileName);

                        if (um.SupportsResizing)
                        {
                            // make default thumbnail
                            um.Resize(100, "thumb");

                            // additional thumbnails configured as prevalues on the DataType
                            if (_thumbnailSizes != "")
                            {
                                char sep = (!_thumbnailSizes.Contains("") && _thumbnailSizes.Contains(",")) ? ',' : ';';

                                foreach (string thumb in _thumbnailSizes.Split(sep))
                                {
                                    int thumbSize;
                                    if (thumb != "" && int.TryParse(thumb, out thumbSize))
                                    {
                                        um.Resize(thumbSize, string.Format("thumb_{0}", thumbSize));
                                    }
                                }
                            }
                        }

                        // check for auto fill of other properties (width, height, extension and filesize)
                        string propertyTypeAlias = new Property(PropertyId).PropertyType.Alias;
                        XmlNode uploadFieldConfigNode =
                            UmbracoSettings.ImageAutoFillImageProperties.SelectSingleNode(
                                string.Format("uploadField [@alias = \"{0}\"]", propertyTypeAlias));
                        if (uploadFieldConfigNode != null)
                        {
                            // get the current document
                            Content content = Content.GetContentFromVersion(Version);
                            // only add dimensions to web images
                            if (um.SupportsResizing)
                            {
                                updateContentProperty(uploadFieldConfigNode, content, "widthFieldAlias",
                                                      um.GetDimensions().Item1);
                                updateContentProperty(uploadFieldConfigNode, content, "heightFieldAlias",
                                                      um.GetDimensions().Item2);
                            }
                            else
                            {
                                updateContentProperty(uploadFieldConfigNode, content, "widthFieldAlias", String.Empty);
                                updateContentProperty(uploadFieldConfigNode, content, "heightFieldAlias", String.Empty);
                            }
                            updateContentProperty(uploadFieldConfigNode, content, "lengthFieldAlias", um.Length);
                            updateContentProperty(uploadFieldConfigNode, content, "extensionFieldAlias", um.Extension);
                        }

                        base.Value = um.LocalName;
                    }
                    else
                    {
                        // if no file is uploaded, we reset the value
                        base.Value = String.Empty;
                    }
                }
                else
                {
                    base.Value = value;
                }
            }
        }

        private void updateContentProperty(XmlNode uploadFieldConfigNode, Content content, string propertyAlias,
                                           object propertyValue)
        {
            XmlNode propertyNode = uploadFieldConfigNode.SelectSingleNode(propertyAlias);
            if (propertyNode != null && !String.IsNullOrEmpty(propertyNode.FirstChild.Value))
            {
                if (content.getProperty(propertyNode.FirstChild.Value) != null)
                {
                    content.getProperty(propertyNode.FirstChild.Value)
                        .Value = propertyValue;
                }
            }
        }
    }
}