using System;
using System.IO;
using System.Web;
using System.Xml;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.Files;
using umbraco.cms.businesslogic.property;
using IContent = Umbraco.Core.Models.IContent;
using IMedia = Umbraco.Core.Models.IMedia;

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

		/// <summary>
		/// Gets/sets the loaded Conent object which we can resolve from other classes since this class sets it's properties
		/// </summary>
		internal Content LoadedContentItem { get; set; }

		/// <summary>
		/// Called to ensure we have a valid LoadedContentItem.
		/// </summary>
		/// <param name="version"></param>
		private void EnsureLoadedContentItem(Guid version)
		{
			if (LoadedContentItem == null)
			{
				LoadedContentItem = Content.GetContentFromVersion(Version);
			}
		}

        public override object Value
        {
            get { return base.Value; }
            set
            {
	            if (value is HttpPostedFile || value is HttpPostedFileBase)
                {
	                Stream fileStream = null;

					var file = value as HttpPostedFile;
					var name = IOHelper.SafeFileName(file.FileName.Substring(file.FileName.LastIndexOf(IOHelper.DirSepChar) + 1, file.FileName.Length - file.FileName.LastIndexOf(IOHelper.DirSepChar) - 1).ToLower());
					fileStream = file.InputStream;

	                // handle upload

                    if (name != String.Empty)
                    {
                        string fileName = UmbracoSettings.UploadAllowDirectories
                                              ? Path.Combine(PropertyId.ToString(), name)
                                              : PropertyId + "-" + name;

                        //fileName = Path.Combine(SystemDirectories.Media, fileName);
                        UmbracoFile um = UmbracoFile.Save(fileStream, fileName);

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
                        if (UmbracoSettings.ImageAutoFillImageProperties != null)
                        {
                            string propertyTypeAlias = new Property(PropertyId).PropertyType.Alias;

                            XmlNode uploadFieldConfigNode =
                                UmbracoSettings.ImageAutoFillImageProperties.SelectSingleNode(
                                    string.Format("uploadField [@alias = \"{0}\"]", propertyTypeAlias));

                            if (uploadFieldConfigNode != null)
                            {
	                            EnsureLoadedContentItem(Version);								
								FillProperties(uploadFieldConfigNode, LoadedContentItem, um);                                
                            }
                        }

                        base.Value = um.Url;
                    }
                    else
                    {
                        // if no file is uploaded, we reset the value
                        base.Value = String.Empty;

                        // also reset values of related fields
                        ClearRelatedValues();
                    }
                }
                else
                {
                    base.Value = value;
                    ClearRelatedValues();
                }
            }
        }

        private void ClearRelatedValues()
        {
            string propertyTypeAlias = new Property(PropertyId).PropertyType.Alias;
            if (UmbracoSettings.ImageAutoFillImageProperties != null)
            {
                XmlNode uploadFieldConfigNode =
                    UmbracoSettings.ImageAutoFillImageProperties.SelectSingleNode(
                        string.Format("uploadField [@alias = \"{0}\"]", propertyTypeAlias));
                if (uploadFieldConfigNode != null)
                {
                    // get the current document
                    //Content legacy = Content.GetContentFromVersion(Version);
	                EnsureLoadedContentItem(Version);
                    // only add dimensions to web images
                    UpdateContentProperty(uploadFieldConfigNode, LoadedContentItem, "widthFieldAlias", String.Empty);
					UpdateContentProperty(uploadFieldConfigNode, LoadedContentItem, "heightFieldAlias", String.Empty);
					UpdateContentProperty(uploadFieldConfigNode, LoadedContentItem, "lengthFieldAlias", String.Empty);
					UpdateContentProperty(uploadFieldConfigNode, LoadedContentItem, "extensionFieldAlias", String.Empty);
                }
            }
        }

        private void FillProperties(XmlNode uploadFieldConfigNode, Content content, UmbracoFile um)
        {
            // only add dimensions to web images
            UpdateContentProperty(uploadFieldConfigNode, content, "widthFieldAlias", um.SupportsResizing ? um.GetDimensions().Item1.ToString() : string.Empty);
            UpdateContentProperty(uploadFieldConfigNode, content, "heightFieldAlias", um.SupportsResizing ? um.GetDimensions().Item2.ToString() : string.Empty);

            UpdateContentProperty(uploadFieldConfigNode, content, "lengthFieldAlias", um.Length);
            UpdateContentProperty(uploadFieldConfigNode, content, "extensionFieldAlias", um.Extension);
        }

        private static void UpdateContentProperty(XmlNode uploadFieldConfigNode, Content content, string propertyAlias,
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