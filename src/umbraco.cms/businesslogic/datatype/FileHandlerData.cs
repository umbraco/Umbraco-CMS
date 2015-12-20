using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Media;
using umbraco.cms.businesslogic.Files;

namespace umbraco.cms.businesslogic.datatype
{
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
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
                    var postedFileName = value is HttpPostedFile
                        ? ((HttpPostedFile)value).FileName
                        : ((HttpPostedFileBase)value).FileName;

                    var name = IOHelper.SafeFileName(postedFileName.Substring(postedFileName.LastIndexOf(IOHelper.DirSepChar) + 1, postedFileName.Length - postedFileName.LastIndexOf(IOHelper.DirSepChar) - 1).ToLower());

                    var fileStream = value is HttpPostedFile
                        ? ((HttpPostedFile)value).InputStream
                        : ((HttpPostedFileBase)value).InputStream;

                    // get the currently stored value, if it is null then change to an empty string.
                    var currentValue = Value == null ? "" : Value.ToString();

                    // handle upload
                    if (name != String.Empty)
                    {
                        var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

                        var subfolder = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                            ? currentValue.Replace(fs.GetUrl("/"), "").Split('/')[0]
                            : currentValue.Substring(currentValue.LastIndexOf("/", StringComparison.Ordinal) + 1).Split('-')[0];
                        
                        int subfolderId;
                        var numberedFolder = int.TryParse(subfolder, out subfolderId)
                            ? subfolderId.ToString(CultureInfo.InvariantCulture)
                            : MediaSubfolderCounter.Current.Increment().ToString(CultureInfo.InvariantCulture);

                        var fileName = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories 
                            ? Path.Combine(numberedFolder, name) 
                            : numberedFolder + "-" + name;
                        
                        var umbracoFile = UmbracoFile.Save(fileStream, fileName);

                        if (umbracoFile.SupportsResizing)
                        {
                            // make default thumbnail
                            umbracoFile.Resize(100, "thumb");

                            // additional thumbnails configured as prevalues on the DataType
                            if (_thumbnailSizes != "")
                            {
								foreach (var thumb in _thumbnailSizes.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int thumbSize;
                                    if (thumb != "" && int.TryParse(thumb, out thumbSize))
                                    {
                                        umbracoFile.Resize(thumbSize, string.Format("thumb_{0}", thumbSize));
                                    }
                                }
                            }
                        }

                        // check for auto fill of other properties (width, height, extension and filesize)
                        if (UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties.Any())
                        {
                            var uploadFieldConfigNode =
                                UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties.FirstOrDefault(x => x.Alias == PropertyTypeAlias);

                            if (uploadFieldConfigNode != null)
                            {
                                EnsureLoadedContentItem(Version);
                                FillProperties(uploadFieldConfigNode, LoadedContentItem, umbracoFile);
                            }
                        }

                        base.Value = umbracoFile.Url;
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
            if (PropertyId == default(int))
                return;

            if (UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties.Any())
            {
                var uploadFieldConfigNode =
                    UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties.FirstOrDefault(x => x.Alias == PropertyTypeAlias);
                if (uploadFieldConfigNode != null)
                {
                    // get the current document
                    //Content legacy = Content.GetContentFromVersion(Version);
                    EnsureLoadedContentItem(Version);

                    var prop = LoadedContentItem.getProperty(uploadFieldConfigNode.WidthFieldAlias);
                    if (prop != null)
                        prop.Value = String.Empty;

                    prop = LoadedContentItem.getProperty(uploadFieldConfigNode.HeightFieldAlias);
                    if (prop != null)
                        prop.Value = String.Empty;

                    prop = LoadedContentItem.getProperty(uploadFieldConfigNode.LengthFieldAlias);
                    if (prop != null)
                        prop.Value = String.Empty;

                    prop = LoadedContentItem.getProperty(uploadFieldConfigNode.ExtensionFieldAlias);
                    if (prop != null)
                        prop.Value = String.Empty;
                }
            }
        }

        private void FillProperties(IImagingAutoFillUploadField uploadFieldConfigNode, Content content, UmbracoFile um)
        {
            var prop = content.getProperty(uploadFieldConfigNode.WidthFieldAlias);
            if (prop != null)
                prop.Value = um.SupportsResizing ? um.GetDimensions().Item1.ToString() : string.Empty;

            prop = content.getProperty(uploadFieldConfigNode.HeightFieldAlias);
            if (prop != null)
                prop.Value = um.SupportsResizing ? um.GetDimensions().Item2.ToString() : string.Empty;

            prop = content.getProperty(uploadFieldConfigNode.LengthFieldAlias);
            if (prop != null)
                prop.Value = um.Length;

            prop = content.getProperty(uploadFieldConfigNode.ExtensionFieldAlias);
            if (prop != null)
                prop.Value = um.Extension;
        }

    }
}