using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Media;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Strings;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    public static class ContentExtensions
    {
        #region IContent

        /// <summary>
        /// Determines if a new version should be created
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// A new version needs to be created when:
        /// * The publish status is changed
        /// * The language is changed
        /// * The item is already published and is being published again and any property value is changed (to enable a rollback)
        /// </remarks>
        internal static bool ShouldCreateNewVersion(this IContent entity)
        {
            var publishedState = ((Content)entity).PublishedState;
            return ShouldCreateNewVersion(entity, publishedState);
        }

        /// <summary>
        /// Determines if a new version should be created
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="publishedState"></param>
        /// <returns></returns>
        /// <remarks>
        /// A new version needs to be created when:
        /// * The publish status is changed
        /// * The language is changed
        /// * The item is already published and is being published again and any property value is changed (to enable a rollback)
        /// </remarks>
        internal static bool ShouldCreateNewVersion(this IContent entity, PublishedState publishedState)
        {
            var dirtyEntity = (ICanBeDirty)entity;
            
            //check if the published state has changed or the language
            var contentChanged =
                (dirtyEntity.IsPropertyDirty("Published") && publishedState != PublishedState.Unpublished)
                || dirtyEntity.IsPropertyDirty("Language");

            //return true if published or language has changed
            if (contentChanged)
            {
                return true;
            }

            //check if any user prop has changed
            var propertyValueChanged = ((Content) entity).IsAnyUserPropertyDirty();
            //check if any content prop has changed
            var contentDataChanged = ((Content) entity).IsEntityDirty();

            //return true if the item is published and a property has changed or if any content property has changed
            return (propertyValueChanged && publishedState == PublishedState.Published) || contentDataChanged;
        }

        /// <summary>
        /// Determines if the published db flag should be set to true for the current entity version and all other db
        /// versions should have their flag set to false.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is determined by:
        /// * If a new version is being created and the entity is published
        /// * If the published state has changed and the entity is published OR the entity has been un-published.
        /// </remarks>
        internal static bool ShouldClearPublishedFlagForPreviousVersions(this IContent entity)
        {
            var publishedState = ((Content)entity).PublishedState;
            return entity.ShouldClearPublishedFlagForPreviousVersions(publishedState, entity.ShouldCreateNewVersion(publishedState));
        }

        /// <summary>
        /// Determines if the published db flag should be set to true for the current entity version and all other db
        /// versions should have their flag set to false.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="publishedState"></param>
        /// <param name="isCreatingNewVersion"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is determined by:
        /// * If a new version is being created and the entity is published
        /// * If the published state has changed and the entity is published OR the entity has been un-published.
        /// </remarks>
        internal static bool ShouldClearPublishedFlagForPreviousVersions(this IContent entity, PublishedState publishedState, bool isCreatingNewVersion)
        {
            if (isCreatingNewVersion && entity.Published)
            {
                return true;
            }

            //If Published state has changed then previous versions should have their publish state reset.
            //If state has been changed to unpublished the previous versions publish state should also be reset.
            if (((ICanBeDirty)entity).IsPropertyDirty("Published") && (entity.Published || publishedState == PublishedState.Unpublished))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a list of the current contents ancestors, not including the content itself.
        /// </summary>
        /// <param name="content">Current content</param>
        /// <returns>An enumerable list of <see cref="IContent"/> objects</returns>
        public static IEnumerable<IContent> Ancestors(this IContent content)
        {
            return ApplicationContext.Current.Services.ContentService.GetAncestors(content);
        }

        /// <summary>
        /// Returns a list of the current contents children.
        /// </summary>
        /// <param name="content">Current content</param>
        /// <returns>An enumerable list of <see cref="IContent"/> objects</returns>
        public static IEnumerable<IContent> Children(this IContent content)
        {
            return ApplicationContext.Current.Services.ContentService.GetChildren(content.Id);
        }

        /// <summary>
        /// Returns a list of the current contents descendants, not including the content itself.
        /// </summary>
        /// <param name="content">Current content</param>
        /// <returns>An enumerable list of <see cref="IContent"/> objects</returns>
        public static IEnumerable<IContent> Descendants(this IContent content)
        {
            return ApplicationContext.Current.Services.ContentService.GetDescendants(content);
        }

        /// <summary>
        /// Returns the parent of the current content.
        /// </summary>
        /// <param name="content">Current content</param>
        /// <returns>An <see cref="IContent"/> object</returns>
        public static IContent Parent(this IContent content)
        {
            return ApplicationContext.Current.Services.ContentService.GetById(content.ParentId);
        }
        #endregion

        #region IMedia
        /// <summary>
        /// Returns a list of the current medias ancestors, not including the media itself.
        /// </summary>
        /// <param name="media">Current media</param>
        /// <returns>An enumerable list of <see cref="IMedia"/> objects</returns>
        public static IEnumerable<IMedia> Ancestors(this IMedia media)
        {
            return ApplicationContext.Current.Services.MediaService.GetAncestors(media);
        }

        /// <summary>
        /// Returns a list of the current medias children.
        /// </summary>
        /// <param name="media">Current media</param>
        /// <returns>An enumerable list of <see cref="IMedia"/> objects</returns>
        public static IEnumerable<IMedia> Children(this IMedia media)
        {
            return ApplicationContext.Current.Services.MediaService.GetChildren(media.Id);
        }

        /// <summary>
        /// Returns a list of the current medias descendants, not including the media itself.
        /// </summary>
        /// <param name="media">Current media</param>
        /// <returns>An enumerable list of <see cref="IMedia"/> objects</returns>
        public static IEnumerable<IMedia> Descendants(this IMedia media)
        {
            return ApplicationContext.Current.Services.MediaService.GetDescendants(media);
        }

        /// <summary>
        /// Returns the parent of the current media.
        /// </summary>
        /// <param name="media">Current media</param>
        /// <returns>An <see cref="IMedia"/> object</returns>
        public static IMedia Parent(this IMedia media)
        {
            return ApplicationContext.Current.Services.MediaService.GetById(media.ParentId);
        }
        #endregion

        /// <summary>
        /// Checks if the IContentBase has children
        /// </summary>
        /// <param name="content"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is a bit of a hack because we need to type check!
        /// </remarks>
        internal static bool HasChildren(IContentBase content, ServiceContext services)
        {
            if (content is IContent)
            {
                return services.ContentService.HasChildren(content.Id);
            }
            if (content is IMedia)
            {
                return services.MediaService.HasChildren(content.Id);
            }
            return false;
        }

        /// <summary>
        /// Returns the children for the content base item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is a bit of a hack because we need to type check!
        /// </remarks>
        internal static IEnumerable<IContentBase> Children(IContentBase content, ServiceContext services)
        {
            if (content is IContent)
            {
                return services.ContentService.GetChildren(content.Id);
            }
            if (content is IMedia)
            {
                return services.MediaService.GetChildren(content.Id);
            }
            return null;
        }

        /// <summary>
        /// Set property values by alias with an annonymous object
        /// </summary>
        public static void PropertyValues(this IContent content, object value)
        {
            if (value == null)
                throw new Exception("No properties has been passed in");

            var propertyInfos = value.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                //Check if a PropertyType with alias exists thus being a valid property
                var propertyType = content.PropertyTypes.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                if (propertyType == null)
                    throw new Exception(
                        string.Format(
                            "The property alias {0} is not valid, because no PropertyType with this alias exists",
                            propertyInfo.Name));

                //Check if a Property with the alias already exists in the collection thus being updated or inserted
                var item = content.Properties.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                if (item != null)
                {
                    item.Value = propertyInfo.GetValue(value, null);
                    //Update item with newly added value
                    content.Properties.Add(item);
                }
                else
                {
                    //Create new Property to add to collection
                    var property = propertyType.CreatePropertyFromValue(propertyInfo.GetValue(value, null));
                    content.Properties.Add(property);
                }
            }
        }

        /// <summary>
        /// Sets and uploads the file from a HttpPostedFileBase object as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="value">The <see cref="HttpPostedFileBase"/> containing the file that will be uploaded</param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFileBase value)
        {
            // Ensure we get the filename without the path in IE in intranet mode 
            // http://stackoverflow.com/questions/382464/httppostedfile-filename-different-from-ie
            var fileName = value.FileName;
            if (fileName.LastIndexOf(@"\") > 0)
                fileName = fileName.Substring(fileName.LastIndexOf(@"\") + 1);

            var name =
                IOHelper.SafeFileName(
                    fileName.Substring(fileName.LastIndexOf(IOHelper.DirSepChar) + 1,
                                       fileName.Length - fileName.LastIndexOf(IOHelper.DirSepChar) - 1)
                            .ToLower());

            if (string.IsNullOrEmpty(name) == false)
                SetFileOnContent(content, propertyTypeAlias, name, value.InputStream);
        }

        /// <summary>
        /// Sets and uploads the file from a HttpPostedFile object as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="value">The <see cref="HttpPostedFile"/> containing the file that will be uploaded</param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFile value)
        {
            // Ensure we get the filename without the path in IE in intranet mode 
            // http://stackoverflow.com/questions/382464/httppostedfile-filename-different-from-ie
            var fileName = value.FileName;
            if (fileName.LastIndexOf(@"\") > 0)
                fileName = fileName.Substring(fileName.LastIndexOf(@"\") + 1);

            var name =
                IOHelper.SafeFileName(
                    fileName.Substring(fileName.LastIndexOf(IOHelper.DirSepChar) + 1,
                                       fileName.Length - fileName.LastIndexOf(IOHelper.DirSepChar) - 1)
                            .ToLower());

            if (string.IsNullOrEmpty(name) == false)
                SetFileOnContent(content, propertyTypeAlias, name, value.InputStream);
        }

        /// <summary>
        /// Sets and uploads the file from a HttpPostedFileWrapper object as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="value">The <see cref="HttpPostedFileWrapper"/> containing the file that will be uploaded</param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFileWrapper value)
        {
            // Ensure we get the filename without the path in IE in intranet mode 
            // http://stackoverflow.com/questions/382464/httppostedfile-filename-different-from-ie
            var fileName = value.FileName;
            if (fileName.LastIndexOf(@"\") > 0)
                fileName = fileName.Substring(fileName.LastIndexOf(@"\") + 1);

            var name = IOHelper.SafeFileName(fileName);

            if (string.IsNullOrEmpty(name) == false)
                SetFileOnContent(content, propertyTypeAlias, name, value.InputStream);
        }

        /// <summary>
        /// Sets and uploads the file from a <see cref="Stream"/> as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileStream"><see cref="Stream"/> to save to disk</param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, string fileName,
                                    Stream fileStream)
        {
            var name = IOHelper.SafeFileName(fileName);

            if (string.IsNullOrEmpty(name) == false && fileStream != null)
                SetFileOnContent(content, propertyTypeAlias, name, fileStream);
        }

        private static void SetFileOnContent(IContentBase content, string propertyTypeAlias, string name, Stream fileStream)
        {
            var property = content.Properties.FirstOrDefault(x => x.Alias == propertyTypeAlias);
            if (property == null)
                return;

            bool supportsResizing = false;
            var numberedFolder = MediaSubfolderCounter.Current.Increment();
            string fileName = UmbracoSettings.UploadAllowDirectories
                                              ? Path.Combine(numberedFolder.ToString(CultureInfo.InvariantCulture), name)
                                              : numberedFolder + "-" + name;

            string extension = Path.GetExtension(name).Substring(1).ToLowerInvariant();

            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            fs.AddFile(fileName, fileStream);

            //Check if file supports resizing and create thumbnails
            if (("," + UmbracoSettings.ImageFileTypes + ",").Contains(string.Format(",{0},", extension)))
            {
                supportsResizing = true;

                // Make default thumbnail
                var thumbUrl = Resize(fs, fileName, extension, 100, "thumb");

                //Look up Prevalues for this upload datatype - if it is an upload datatype
                var uploadFieldId = new Guid(Constants.PropertyEditors.UploadField);
                if (property.PropertyType.DataTypeId == uploadFieldId)
                {
                    //Get Prevalues by the DataType's Id: property.PropertyType.DataTypeId
                    var values = ApplicationContext.Current.Services.DataTypeService.GetPreValuesByDataTypeId(property.PropertyType.DataTypeDefinitionId);
                    var thumbnailSizes = values.FirstOrDefault();
                    //Additional thumbnails configured as prevalues on the DataType
                    if (thumbnailSizes != null)
                    {
                        char sep = (!thumbnailSizes.Contains("") && thumbnailSizes.Contains(",")) ? ',' : ';';

                        foreach (string thumb in thumbnailSizes.Split(sep))
                        {
                            int thumbSize;
                            if (thumb != "" && int.TryParse(thumb, out thumbSize))
                            {
                                Resize(fs, fileName, extension, thumbSize, string.Format("thumb_{0}", thumbSize));
                            }
                        }
                    }
                }
            }

            //Check for auto fill of additional properties
            if (UmbracoSettings.ImageAutoFillImageProperties != null)
            {
                XmlNode uploadFieldConfigNode =
                    UmbracoSettings.ImageAutoFillImageProperties.SelectSingleNode(
                        string.Format("uploadField [@alias = \"{0}\"]", propertyTypeAlias));

                if (uploadFieldConfigNode != null)
                {
                    //Only add dimensions to web images
                    if (supportsResizing)
                    {
                        SetPropertyValue(content, uploadFieldConfigNode, "widthFieldAlias", GetDimensions(fs, fileName).Item1.ToString(CultureInfo.InvariantCulture));
                        SetPropertyValue(content, uploadFieldConfigNode, "heightFieldAlias", GetDimensions(fs, fileName).Item2.ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        SetPropertyValue(content, uploadFieldConfigNode, "widthFieldAlias", string.Empty);
                        SetPropertyValue(content, uploadFieldConfigNode, "heightFieldAlias", string.Empty);
                    }

                    SetPropertyValue(content, uploadFieldConfigNode, "lengthFieldAlias", fs.GetSize(fileName).ToString(CultureInfo.InvariantCulture));
                    SetPropertyValue(content, uploadFieldConfigNode, "extensionFieldAlias", extension);
                }
            }

            //Set the value of the property to that of the uploaded file's url
            property.Value = fs.GetUrl(fileName);
        }

        private static void SetPropertyValue(IContentBase content, XmlNode uploadFieldConfigNode, string propertyAlias, string propertyValue)
        {
            XmlNode propertyNode = uploadFieldConfigNode.SelectSingleNode(propertyAlias);
            if (propertyNode != null && string.IsNullOrEmpty(propertyNode.FirstChild.Value) == false && content.HasProperty(propertyNode.FirstChild.Value))
            {
                content.SetValue(propertyNode.FirstChild.Value, propertyValue);
            }
        }

        private static string Resize(MediaFileSystem fileSystem, string path, string extension, int maxWidthHeight, string fileNameAddition)
        {
            var fileNameThumb = DoResize(fileSystem, path, extension, GetDimensions(fileSystem, path).Item1, GetDimensions(fileSystem, path).Item2, maxWidthHeight, fileNameAddition);

            return fileSystem.GetUrl(fileNameThumb);
        }

        private static Tuple<int, int> GetDimensions(MediaFileSystem fileSystem, string path)
        {
            var fs = fileSystem.OpenFile(path);
            var image = Image.FromStream(fs);
            var fileWidth = image.Width;
            var fileHeight = image.Height;
            fs.Close();
            image.Dispose();

            return new Tuple<int, int>(fileWidth, fileHeight);
        }

        private static string DoResize(MediaFileSystem fileSystem, string path, string extension, int width, int height, int maxWidthHeight, string fileNameAddition)
        {
            var fs = fileSystem.OpenFile(path);
            var image = Image.FromStream(fs);
            fs.Close();

            string fileNameThumb = String.IsNullOrEmpty(fileNameAddition) ?
                string.Format("{0}_UMBRACOSYSTHUMBNAIL.jpg", path.Substring(0, path.LastIndexOf("."))) :
                string.Format("{0}_{1}.jpg", path.Substring(0, path.LastIndexOf(".")), fileNameAddition);

            var thumb = GenerateThumbnail(fileSystem,
                image,
                maxWidthHeight,
                width,
                height,
                path,
                extension,
                fileNameThumb,
                maxWidthHeight == 0);

            fileNameThumb = thumb.Item3;

            image.Dispose();

            return fileNameThumb;
        }

        private static Tuple<int, int, string> GenerateThumbnail(MediaFileSystem fileSystem, Image image, int maxWidthHeight, int fileWidth,
                                                int fileHeight, string fullFilePath, string extension,
                                                string thumbnailFileName, bool useFixedDimensions)
        {
            // Generate thumbnail
            float f = 1;
            if (!useFixedDimensions)
            {
                var fx = (float)image.Size.Width / (float)maxWidthHeight;
                var fy = (float)image.Size.Height / (float)maxWidthHeight;

                // must fit in thumbnail size
                f = Math.Max(fx, fy); //if (f < 1) f = 1;
            }

            var widthTh = (int)Math.Round((float)fileWidth / f); int heightTh = (int)Math.Round((float)fileHeight / f);

            // fixes for empty width or height
            if (widthTh == 0)
                widthTh = 1;
            if (heightTh == 0)
                heightTh = 1;

            // Create new image with best quality settings
            var bp = new Bitmap(widthTh, heightTh);
            var g = Graphics.FromImage(bp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Copy the old image to the new and resized
            var rect = new Rectangle(0, 0, widthTh, heightTh);
            g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

            // Copy metadata
            var imageEncoders = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo codec = null;
            if (extension.ToLower() == "png" || extension.ToLower() == "gif")
                codec = imageEncoders.Single(t => t.MimeType.Equals("image/png"));
            else
                codec = imageEncoders.Single(t => t.MimeType.Equals("image/jpeg"));


            // Set compresion ratio to 90%
            var ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

            // Save the new image using the dimensions of the image
            string newFileName = thumbnailFileName.Replace("UMBRACOSYSTHUMBNAIL",
                                                           string.Format("{0}x{1}", widthTh, heightTh));
            var ms = new MemoryStream();
            bp.Save(ms, codec, ep);
            ms.Seek(0, 0);

            fileSystem.AddFile(newFileName, ms);

            ms.Close();
            bp.Dispose();
            g.Dispose();

            return new Tuple<int, int, string>(widthTh, heightTh, newFileName);
        }

		/// <summary>
		/// Gets the <see cref="IProfile"/> for the Creator of this media item.
		/// </summary>
		public static IProfile GetCreatorProfile(this IMedia media)
		{
            return ApplicationContext.Current.Services.UserService.GetProfileById(media.CreatorId);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Creator of this content item.
        /// </summary>
        public static IProfile GetCreatorProfile(this IContentBase content)
        {
            return ApplicationContext.Current.Services.UserService.GetProfileById(content.CreatorId);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Writer of this content.
        /// </summary>
        public static IProfile GetWriterProfile(this IContent content)
        {
            return ApplicationContext.Current.Services.UserService.GetProfileById(content.WriterId);
        }

        /// <summary>
        /// Checks whether an <see cref="IContent"/> item has any published versions
        /// </summary>
        /// <param name="content"></param>
        /// <returns>True if the content has any published versiom otherwise False</returns>
        public static bool HasPublishedVersion(this IContent content)
        {
            if (content.HasIdentity == false)
                return false;

            return ApplicationContext.Current.Services.ContentService.HasPublishedVersion(content.Id);
        }

        /// <summary>
        /// Creates the full xml representation for the <see cref="IContent"/> object and all of it's descendants
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        internal static XElement ToDeepXml(this IContent content)
        {
            return ApplicationContext.Current.Services.PackagingService.Export(content, true);
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content)
        {
            return ApplicationContext.Current.Services.PackagingService.Export(content);
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media"><see cref="IContent"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IMedia media)
        {
            return ApplicationContext.Current.Services.PackagingService.Export(media);
        }

        internal static XElement ToDeepXml(this IMedia media)
        {
            return ApplicationContext.Current.Services.PackagingService.Export(media, true);
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <param name="isPreview">Boolean indicating whether the xml should be generated for preview</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content, bool isPreview)
        {
            //TODO Do a proper implementation of this
            //If current IContent is published we should get latest unpublished version
            return content.ToXml();
        }
    }
}