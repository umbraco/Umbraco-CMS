using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Media;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    public static class ContentExtensions
    {
        #region IContent

        /// <summary>
        /// Returns true if this entity was just published as part of a recent save operation (i.e. it wasn't previously published)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is helpful for determining if the published event will execute during the saved event for a content item.
        /// </remarks>
        internal static bool JustPublished(this IContent entity)
        {
            var dirty = (IRememberBeingDirty)entity;
            return dirty.WasPropertyDirty("Published") && entity.Published;
        }

        /// <summary>
        /// Determines whether the content should be persisted.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <returns>True is the content should be persisted, otherwise false.</returns>
        /// <remarks>See remarks in overload.</remarks>
        internal static bool RequiresSaving(this IContent entity)
        {
            return RequiresSaving(entity, ((Content) entity).PublishedState);
        }

        /// <summary>
        /// Determines whether the content should be persisted.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <param name="publishedState">The published state of the content.</param>
        /// <returns>True is the content should be persisted, otherwise false.</returns>
        /// <remarks>
        /// This is called by the repository when persisting an existing content, to
        /// figure out whether it needs to persist the content at all.
        /// </remarks>
        internal static bool RequiresSaving(this IContent entity, PublishedState publishedState)
        {
            // note: publishedState is always the entity's PublishedState except for tests

            var content = (Content) entity;
            var userPropertyChanged = content.IsAnyUserPropertyDirty();
            var dirtyProps = content.GetDirtyProperties();
            //var contentPropertyChanged = content.IsEntityDirty();
            var contentPropertyChangedExceptPublished = dirtyProps.Any(x => x != "Published");

            // we don't want to save (write to DB) if we are "saving" either a published content
            // (.Saving) or an unpublished content (.Unpublished) and strictly nothing has changed

            var noSave = (publishedState == PublishedState.Saving || publishedState == PublishedState.Unpublished)
                && userPropertyChanged == false
                && contentPropertyChangedExceptPublished == false;

            return noSave == false;
        }

        /// <summary>
        /// Determines whether a new version of the content should be created.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <returns>True if a new version should be created, otherwise false.</returns>
        /// <remarks>See remarks in overload.</remarks>
        internal static bool RequiresNewVersion(this IContent entity)
        {
            return RequiresNewVersion(entity, ((Content) entity).PublishedState);
        }

        /// <summary>
        /// Determines whether a new version of the content should be created.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <param name="publishedState">The published state of the content.</param>
        /// <returns>True if a new version should be created, otherwise false.</returns>
        /// <remarks>
        /// This is called by the repository when persisting an existing content, to
        /// figure out whether it needs to create a new version for that content.
        /// A new version needs to be created when:
        /// * The publish status is changed
        /// * The language is changed
        /// * A content property is changed (? why ?)
        /// * The item is already published and is being published again and any property value is changed (to enable a rollback)
        /// </remarks>
        internal static bool RequiresNewVersion(this IContent entity, PublishedState publishedState)
        {
            // note: publishedState is always the entity's PublishedState except for tests

            // read
            // http://issues.umbraco.org/issue/U4-2589 (save & publish & creating new versions)
            // http://issues.umbraco.org/issue/U4-3404 (pressing preview does save then preview)
            // http://issues.umbraco.org/issue/U4-5510 (previewing & creating new versions)
            //
            // slightly modifying the rules to make more sense (marked with CHANGE)
            // but should respect the result of the discussions in those issues

            // figure out whether .Language has changed
            // this language stuff was an old POC and should be removed
            var hasLanguageChanged = entity.IsPropertyDirty("Language");
            if (hasLanguageChanged)
                return true; // language change => new version

            var content = (Content) entity;
            //var contentPropertyChanged = content2.IsEntityDirty();
            var userPropertyChanged = content.IsAnyUserPropertyDirty();
            var dirtyProps = content.GetDirtyProperties();
            var contentPropertyChangedExceptPublished = dirtyProps.Any(x => x != "Published");
            var wasPublished = content.PublishedOriginal;

            switch (publishedState)
            {
                case PublishedState.Publishing:
                    // changed state, publishing either a published or an unpublished version:
                    // DO create a new (published) version IF it was published already AND
                    // anything has changed, else can reuse the current version
                    return (contentPropertyChangedExceptPublished || userPropertyChanged) && wasPublished;

                case PublishedState.Unpublishing:
                    // changed state, unpublishing a published version:
                    // DO create a new (draft) version and preserve the (formerly) published
                    // version for rollback purposes IF the version that's being saved is the
                    // published version, else it's a draft that we can reuse
                    return wasPublished;

                case PublishedState.Saving:
                    // changed state, saving a published version:
                    // DO create a new (draft) version and preserve the published version IF
                    // anything has changed, else do NOT create a new version (pointless)
                    return contentPropertyChangedExceptPublished || userPropertyChanged;

                case PublishedState.Published:
                    // unchanged state, saving a published version:
                    // (can happen eg when moving content, never otherwise)
                    // do NOT create a new version as we're just saving after operations (eg
                    // move) that cannot be rolled back anyway - ensure that's really it
                    if (userPropertyChanged)
                        throw new InvalidOperationException("Invalid PublishedState \"Published\" with user property changes.");
                    return false;

                case PublishedState.Unpublished:
                    // unchanged state, saving an unpublished version:
                    // do NOT create a new version for user property changes,
                    // BUT create a new version in case of content property changes, for
                    // rollback purposes
                    return contentPropertyChangedExceptPublished;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Determines whether the database published flag should be cleared for versions
        /// other than this content version.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <returns>True if the published flag should be cleared, otherwise false.</returns>
        /// <returns>See remarks in overload.</returns>
        internal static bool RequiresClearPublishedFlag(this IContent entity)
        {
            var publishedState = ((Content) entity).PublishedState;
            var requiresNewVersion = entity.RequiresNewVersion(publishedState);
            return entity.RequiresClearPublishedFlag(publishedState, requiresNewVersion);
        }

        /// <summary>
        /// Determines whether the database published flag should be cleared for versions
        /// other than this content version.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <param name="publishedState">The published state of the content.</param>
        /// <param name="isNewVersion">Indicates whether the content is a new version.</param>
        /// <returns>True if the published flag should be cleared, otherwise false.</returns>
        /// <remarks>
        /// This is called by the repository when persisting an existing content, to
        /// figure out whether it needs to clear the published flag for other versions.
        /// </remarks>
        internal static bool RequiresClearPublishedFlag(this IContent entity, PublishedState publishedState, bool isNewVersion)
        {
            // note: publishedState is always the entity's PublishedState except for tests

            // new, published version => everything else must be cleared
            if (isNewVersion && entity.Published)
                return true;

            // if that entity was published then that entity has the flag and
            // it does not need to be cleared for other versions
            // NOT TRUE when unpublishing we create a NEW version
            //var wasPublished = ((Content)entity).PublishedOriginal;
            //if (wasPublished)
            //    return false;

            // clear whenever we are publishing or unpublishing
            //  publishing: because there might be a previously published version, which needs to be cleared
            //  unpublishing: same - we might be a saved version, not the published one, which needs to be cleared
            return publishedState == PublishedState.Publishing || publishedState == PublishedState.Unpublishing;
        }

        /// <summary>
        /// Returns a list of the current contents ancestors, not including the content itself.
        /// </summary>
        /// <param name="content">Current content</param>
        /// <param name="contentService"></param>
        /// <returns>An enumerable list of <see cref="IContent"/> objects</returns>
        public static IEnumerable<IContent> Ancestors(this IContent content, IContentService contentService)
        {
            return contentService.GetAncestors(content);
        }

        [Obsolete("Use the overload with the service reference instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<IContent> Ancestors(this IContent content)
        {
            return ApplicationContext.Current.Services.ContentService.GetAncestors(content);
        }

        /// <summary>
        /// Returns a list of the current contents children.
        /// </summary>
        /// <param name="content">Current content</param>
        /// <param name="contentService"></param>
        /// <returns>An enumerable list of <see cref="IContent"/> objects</returns>
        public static IEnumerable<IContent> Children(this IContent content, IContentService contentService)
        {
            return contentService.GetChildren(content.Id);
        }

        [Obsolete("Use the overload with the service reference instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<IContent> Children(this IContent content)
        {
            return ApplicationContext.Current.Services.ContentService.GetChildren(content.Id);
        }

        /// <summary>
        /// Returns a list of the current contents descendants, not including the content itself.
        /// </summary>
        /// <param name="content">Current content</param>
        /// <param name="contentService"></param>
        /// <returns>An enumerable list of <see cref="IContent"/> objects</returns>
        public static IEnumerable<IContent> Descendants(this IContent content, IContentService contentService)
        {
            return contentService.GetDescendants(content);
        }

        [Obsolete("Use the overload with the service reference instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<IContent> Descendants(this IContent content)
        {
            return ApplicationContext.Current.Services.ContentService.GetDescendants(content);
        }

        /// <summary>
        /// Returns the parent of the current content.
        /// </summary>
        /// <param name="content">Current content</param>
        /// <param name="contentService"></param>
        /// <returns>An <see cref="IContent"/> object</returns>
        public static IContent Parent(this IContent content, IContentService contentService)
        {
            return contentService.GetById(content.ParentId);
        }

        [Obsolete("Use the overload with the service reference instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
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
        /// <param name="mediaService"></param>
        /// <returns>An enumerable list of <see cref="IMedia"/> objects</returns>
        public static IEnumerable<IMedia> Ancestors(this IMedia media, IMediaService mediaService)
        {
            return mediaService.GetAncestors(media);
        }

        [Obsolete("Use the overload with the service reference instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<IMedia> Ancestors(this IMedia media)
        {
            return ApplicationContext.Current.Services.MediaService.GetAncestors(media);
        }

        /// <summary>
        /// Returns a list of the current medias children.
        /// </summary>
        /// <param name="media">Current media</param>
        /// <param name="mediaService"></param>
        /// <returns>An enumerable list of <see cref="IMedia"/> objects</returns>
        public static IEnumerable<IMedia> Children(this IMedia media, IMediaService mediaService)
        {
            return mediaService.GetChildren(media.Id);
        }

        [Obsolete("Use the overload with the service reference instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<IMedia> Children(this IMedia media)
        {
            return ApplicationContext.Current.Services.MediaService.GetChildren(media.Id);
        }

        /// <summary>
        /// Returns a list of the current medias descendants, not including the media itself.
        /// </summary>
        /// <param name="media">Current media</param>
        /// <param name="mediaService"></param>
        /// <returns>An enumerable list of <see cref="IMedia"/> objects</returns>
        public static IEnumerable<IMedia> Descendants(this IMedia media, IMediaService mediaService)
        {
            return mediaService.GetDescendants(media);
        }

        [Obsolete("Use the overload with the service reference instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<IMedia> Descendants(this IMedia media)
        {
            return ApplicationContext.Current.Services.MediaService.GetDescendants(media);
        }

        /// <summary>
        /// Returns the parent of the current media.
        /// </summary>
        /// <param name="media">Current media</param>
        /// <param name="mediaService"></param>
        /// <returns>An <see cref="IMedia"/> object</returns>
        public static IMedia Parent(this IMedia media, IMediaService mediaService)
        {
            return mediaService.GetById(media.ParentId);
        }

        [Obsolete("Use the overload with the service reference instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IMedia Parent(this IMedia media)
        {
            return ApplicationContext.Current.Services.MediaService.GetById(media.ParentId);
        }
        #endregion

        internal static bool IsInRecycleBin(this IContent content)
        {
            return IsInRecycleBin(content, Constants.System.RecycleBinContent);
        }

        internal static bool IsInRecycleBin(this IMedia media)
        {
            return IsInRecycleBin(media, Constants.System.RecycleBinMedia);
        }

        internal static bool IsInRecycleBin(this IContentBase content, int recycleBinId)
        {
            return content.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                          .Contains(recycleBinId.ToInvariantString());
        }

        /// <summary>
        /// Removes characters that are not valide XML characters from all entity properties
        /// of type string. See: http://stackoverflow.com/a/961504/5018
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If this is not done then the xml cache can get corrupt and it will throw YSODs upon reading it.
        /// </remarks>
        /// <param name="entity"></param>
        public static void SanitizeEntityPropertiesForXmlStorage(this IContentBase entity)
        {
            entity.Name = entity.Name.ToValidXmlString();
            foreach (var property in entity.Properties)
            {
                if (property.Value is string)
                {
                    var value = (string)property.Value;
                    property.Value = value.ToValidXmlString();
                }
            }
        }

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
        /// Returns properties that do not belong to a group
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IEnumerable<Property> GetNonGroupedProperties(this IContentBase content)
        {
            var propertyIdsInTabs = content.PropertyGroups.SelectMany(pg => pg.PropertyTypes);
            return content.Properties
                          .Where(property => propertyIdsInTabs.Contains(property.PropertyType) == false)
                          .OrderBy(x => x.PropertyType.SortOrder);
        }

        /// <summary>
        /// Returns the Property object for the given property group
        /// </summary>
        /// <param name="content"></param>
        /// <param name="propertyGroup"></param>
        /// <returns></returns>
        public static IEnumerable<Property> GetPropertiesForGroup(this IContentBase content, PropertyGroup propertyGroup)
        {
            //get the properties for the current tab
            return content.Properties
                          .Where(property => propertyGroup.PropertyTypes
                                                          .Select(propertyType => propertyType.Id)
                                                          .Contains(property.PropertyTypeId))
                          .OrderBy(x => x.PropertyType.SortOrder);
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

        #region SetValue for setting file contents

        /// <summary>
        /// Sets and uploads the file from a HttpPostedFileBase object as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="value">The <see cref="HttpPostedFileBase"/> containing the file that will be uploaded</param>
        /// <param name="dataTypeService"></param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFileBase value, IDataTypeService dataTypeService)
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
                SetFileOnContent(content, propertyTypeAlias, name, value.InputStream, dataTypeService);
        }

        [Obsolete("Use the overload with the IDataTypeService parameter instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFileBase value)
        {
            content.SetValue(propertyTypeAlias, value, ApplicationContext.Current.Services.DataTypeService);
        }

        /// <summary>
        /// Sets and uploads the file from a HttpPostedFile object as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="value">The <see cref="HttpPostedFile"/> containing the file that will be uploaded</param>
        /// <param name="dataTypeService"></param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFile value, IDataTypeService dataTypeService)
        {
            SetValue(content, propertyTypeAlias, new HttpPostedFileWrapper(value), dataTypeService);
        }

        [Obsolete("Use the overload with the IDataTypeService parameter instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFile value)
        {
            SetValue(content, propertyTypeAlias, (HttpPostedFileBase)new HttpPostedFileWrapper(value));
        }

        /// <summary>
        /// Sets and uploads the file from a HttpPostedFileWrapper object as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="value">The <see cref="HttpPostedFileWrapper"/> containing the file that will be uploaded</param>
        [Obsolete("There is no reason for this overload since HttpPostedFileWrapper inherits from HttpPostedFileBase")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFileWrapper value)
        {
            SetValue(content, propertyTypeAlias, (HttpPostedFileBase)value);
        }

        /// <summary>
        /// Sets and uploads the file from a <see cref="Stream"/> as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileStream"><see cref="Stream"/> to save to disk</param>
        /// <param name="dataTypeService"></param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, string fileName, Stream fileStream, IDataTypeService dataTypeService)
        {
            var name = IOHelper.SafeFileName(fileName);

            if (string.IsNullOrEmpty(name) == false && fileStream != null)
                SetFileOnContent(content, propertyTypeAlias, name, fileStream, dataTypeService);
        }

        [Obsolete("Use the overload with the IDataTypeService parameter instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetValue(this IContentBase content, string propertyTypeAlias, string fileName, Stream fileStream)
        {
            content.SetValue(propertyTypeAlias, fileName, fileStream, ApplicationContext.Current.Services.DataTypeService);
        }

        private static void SetFileOnContent(IContentBase content, string propertyTypeAlias, string filename, Stream fileStream, IDataTypeService dataTypeService)
        {
            var property = content.Properties.FirstOrDefault(x => x.Alias == propertyTypeAlias);
            if (property == null)
                return;

            //TODO: ALl of this naming logic needs to be put into the ImageHelper and then we need to change FileUploadPropertyValueEditor to do the same!

            var numberedFolder = MediaSubfolderCounter.Current.Increment();
            var fileName = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                                              ? Path.Combine(numberedFolder.ToString(CultureInfo.InvariantCulture), filename)
                                              : numberedFolder + "-" + filename;

            var extension = Path.GetExtension(filename).Substring(1).ToLowerInvariant();

            //the file size is the length of the stream in bytes
            var fileSize = fileStream.Length;

            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            fs.AddFile(fileName, fileStream);

            //Check if file supports resizing and create thumbnails
            var supportsResizing = UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.InvariantContains(extension);

            //the config section used to auto-fill properties
            IImagingAutoFillUploadField uploadFieldConfigNode = null;

            //Check for auto fill of additional properties
            if (UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties != null)
            {
                uploadFieldConfigNode = UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties
                                    .FirstOrDefault(x => x.Alias == propertyTypeAlias);

            }

            if (supportsResizing)
            {
                //get the original image from the original stream
                if (fileStream.CanSeek) fileStream.Seek(0, 0);
                using (var originalImage = Image.FromStream(fileStream))
                {
                    var additionalSizes = new List<int>();

                    //Look up Prevalues for this upload datatype - if it is an upload datatype - get additional configured sizes
                    if (property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias)
                    {
                        //Get Prevalues by the DataType's Id: property.PropertyType.DataTypeId
                        var values = dataTypeService.GetPreValuesByDataTypeId(property.PropertyType.DataTypeDefinitionId);
                        var thumbnailSizes = values.FirstOrDefault();
                        //Additional thumbnails configured as prevalues on the DataType
                        if (thumbnailSizes != null)
                        {
							foreach (var thumb in thumbnailSizes.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                int thumbSize;
                                if (thumb != "" && int.TryParse(thumb, out thumbSize))
                                {
                                    additionalSizes.Add(thumbSize);
                                }
                            }
                        }
                    }

                    ImageHelper.GenerateMediaThumbnails(fs, fileName, extension, originalImage, additionalSizes);

                    //while the image is still open, we'll check if we need to auto-populate the image properties
                    if (uploadFieldConfigNode != null)
                    {
                        content.SetValue(uploadFieldConfigNode.WidthFieldAlias, originalImage.Width.ToString(CultureInfo.InvariantCulture));
                        content.SetValue(uploadFieldConfigNode.HeightFieldAlias, originalImage.Height.ToString(CultureInfo.InvariantCulture));
                    }

                }
            }

            //if auto-fill is true, then fill the remaining, non-image properties
            if (uploadFieldConfigNode != null)
            {
                content.SetValue(uploadFieldConfigNode.LengthFieldAlias, fileSize.ToString(CultureInfo.InvariantCulture));
                content.SetValue(uploadFieldConfigNode.ExtensionFieldAlias, extension);
            }

            //Set the value of the property to that of the uploaded file's url
            property.Value = fs.GetUrl(fileName);
        }

        #endregion

        #region User/Profile methods

        
        [Obsolete("Use the overload that declares the IUserService to use")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IProfile GetCreatorProfile(this IMedia media)
        {
            return ApplicationContext.Current.Services.UserService.GetProfileById(media.CreatorId);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Creator of this media item.
        /// </summary>
        public static IProfile GetCreatorProfile(this IMedia media, IUserService userService)
        {
            return userService.GetProfileById(media.CreatorId);
        }
        
        [Obsolete("Use the overload that declares the IUserService to use")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IProfile GetCreatorProfile(this IContentBase content)
        {
            return ApplicationContext.Current.Services.UserService.GetProfileById(content.CreatorId);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Creator of this content item.
        /// </summary>
        public static IProfile GetCreatorProfile(this IContentBase content, IUserService userService)
        {
            return userService.GetProfileById(content.CreatorId);
        }
        
        [Obsolete("Use the overload that declares the IUserService to use")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IProfile GetWriterProfile(this IContent content)
        {
            return ApplicationContext.Current.Services.UserService.GetProfileById(content.WriterId);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Writer of this content.
        /// </summary>
        public static IProfile GetWriterProfile(this IContent content, IUserService userService)
        {
            return userService.GetProfileById(content.WriterId);
        }

        #endregion

        /// <summary>
        /// Checks whether an <see cref="IContent"/> item has any published versions
        /// </summary>
        /// <param name="content"></param>
        /// <returns>True if the content has any published versiom otherwise False</returns>
        [Obsolete("Use the HasPublishedVersion property.", false)]
        public static bool HasPublishedVersion(this IContent content)
        {
            return content.HasPublishedVersion;
        }

        #region Tag methods

       

        /// <summary>
        /// Sets tags for the property - will add tags to the tags table and set the property value to be the comma delimited value of the tags.
        /// </summary>
        /// <param name="content">The content item to assign the tags to</param>
        /// <param name="propertyTypeAlias">The property alias to assign the tags to</param>
        /// <param name="tags">The tags to assign</param>
        /// <param name="replaceTags">True to replace the tags on the current property with the tags specified or false to merge them with the currently assigned ones</param>
        /// <param name="tagGroup">The group/category to assign the tags, the default value is "default"</param>
        /// <returns></returns>
        public static void SetTags(this IContentBase content, string propertyTypeAlias, IEnumerable<string> tags, bool replaceTags, string tagGroup = "default")
        {
            content.SetTags(TagCacheStorageType.Csv, propertyTypeAlias, tags, replaceTags, tagGroup);
        }

        /// <summary>
        /// Sets tags for the property - will add tags to the tags table and set the property value to be the comma delimited value of the tags.
        /// </summary>
        /// <param name="content">The content item to assign the tags to</param>
        /// <param name="storageType">The tag storage type in cache (default is csv)</param>
        /// <param name="propertyTypeAlias">The property alias to assign the tags to</param>
        /// <param name="tags">The tags to assign</param>
        /// <param name="replaceTags">True to replace the tags on the current property with the tags specified or false to merge them with the currently assigned ones</param>
        /// <param name="tagGroup">The group/category to assign the tags, the default value is "default"</param>
        /// <returns></returns>
        public static void SetTags(this IContentBase content, TagCacheStorageType storageType, string propertyTypeAlias, IEnumerable<string> tags, bool replaceTags, string tagGroup = "default")
        {
            var property = content.Properties[propertyTypeAlias];
            if (property == null)
            {
                throw new IndexOutOfRangeException("No property exists with name " + propertyTypeAlias);
            }
            property.SetTags(storageType, propertyTypeAlias, tags, replaceTags, tagGroup);
        }

        internal static void SetTags(this Property property, TagCacheStorageType storageType, string propertyTypeAlias, IEnumerable<string> tags, bool replaceTags, string tagGroup = "default")
        {
            if (property == null) throw new ArgumentNullException("property");

            var trimmedTags = tags.Select(x => x.Trim()).ToArray();

            property.TagSupport.Enable = true;
            property.TagSupport.Tags = trimmedTags.Select(x => new Tuple<string, string>(x, tagGroup));
            property.TagSupport.Behavior = replaceTags ? PropertyTagBehavior.Replace : PropertyTagBehavior.Merge;

            //ensure the property value is set to the same thing
            if (replaceTags)
            {
                switch (storageType)
                {
                    case TagCacheStorageType.Csv:
                        property.Value = string.Join(",", trimmedTags);
                        break;
                    case TagCacheStorageType.Json:
                        //json array
                        property.Value = JsonConvert.SerializeObject(trimmedTags);
                        break;
                }

            }
            else
            {
                switch (storageType)
                {
                    case TagCacheStorageType.Csv:
                        var currTags = property.Value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(x => x.Trim());
                        property.Value = string.Join(",", trimmedTags.Union(currTags));
                        break;
                    case TagCacheStorageType.Json:
                        var currJson = JsonConvert.DeserializeObject<JArray>(property.Value.ToString());
                        //need to append the new ones
                        foreach (var tag in trimmedTags)
                        {
                            currJson.Add(tag);
                        }
                        //json array
                        property.Value = JsonConvert.SerializeObject(currJson);
                        break;
                }
            }
        }

        /// <summary>
        /// Remove any of the tags specified in the collection from the property if they are currently assigned.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="tags"></param>
        /// <param name="tagGroup">The group/category that the tags are currently assigned to, the default value is "default"</param>
        public static void RemoveTags(this IContentBase content, string propertyTypeAlias, IEnumerable<string> tags, string tagGroup = "default")
        {
            var property = content.Properties[propertyTypeAlias];
            if (property == null)
            {
                throw new IndexOutOfRangeException("No property exists with name " + propertyTypeAlias);
            }

            var trimmedTags = tags.Select(x => x.Trim()).ToArray();

            property.TagSupport.Behavior = PropertyTagBehavior.Remove;
            property.TagSupport.Enable = true;
            property.TagSupport.Tags = trimmedTags.Select(x => new Tuple<string, string>(x, tagGroup));

            //set the property value
            var currTags = property.Value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(x => x.Trim());

            property.Value = string.Join(",", currTags.Except(trimmedTags));
        }

        #endregion

        #region XML methods

        /// <summary>
        /// Creates the full xml representation for the <see cref="IContent"/> object and all of it's descendants
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <param name="packagingService"></param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        internal static XElement ToDeepXml(this IContent content, IPackagingService packagingService)
        {
            return packagingService.Export(content, true, raiseEvents: false);
        }

     
        [Obsolete("Use the overload that declares the IPackagingService to use")]
        public static XElement ToXml(this IContent content)
        {
            return ApplicationContext.Current.Services.PackagingService.Export(content, raiseEvents: false);
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <param name="packagingService"></param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content, IPackagingService packagingService)
        {
            return packagingService.Export(content, raiseEvents: false);
        }

        [Obsolete("Use the overload that declares the IPackagingService to use")]
        public static XElement ToXml(this IMedia media)
        {
            return ApplicationContext.Current.Services.PackagingService.Export(media, raiseEvents: false);
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media"><see cref="IContent"/> to generate xml for</param>
        /// <param name="packagingService"></param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IMedia media, IPackagingService packagingService)
        {
            return packagingService.Export(media, raiseEvents: false);
        }

        /// <summary>
        /// Creates the full xml representation for the <see cref="IMedia"/> object and all of it's descendants
        /// </summary>
        /// <param name="media"><see cref="IMedia"/> to generate xml for</param>
        /// <param name="packagingService"></param>
        /// <returns>Xml representation of the passed in <see cref="IMedia"/></returns>
        internal static XElement ToDeepXml(this IMedia media, IPackagingService packagingService)
        {
            return packagingService.Export(media, true, raiseEvents: false);
        }

        [Obsolete("Use the overload that declares the IPackagingService to use")]
        public static XElement ToXml(this IContent content, bool isPreview)
        {
            //TODO Do a proper implementation of this
            //If current IContent is published we should get latest unpublished version
            return content.ToXml();
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <param name="packagingService"></param>
        /// <param name="isPreview">Boolean indicating whether the xml should be generated for preview</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content, IPackagingService packagingService, bool isPreview)
        {
            //TODO Do a proper implementation of this
            //If current IContent is published we should get latest unpublished version
            return content.ToXml(packagingService);
        }

        [Obsolete("Use the overload that declares the IPackagingService to use")]
        public static XElement ToXml(this IMember member)
        {
            return ((PackagingService)(ApplicationContext.Current.Services.PackagingService)).Export(member);
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IMember"/> object
        /// </summary>
        /// <param name="member"><see cref="IMember"/> to generate xml for</param>
        /// <param name="packagingService"></param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IMember member, IPackagingService packagingService)
        {
            return ((PackagingService)(packagingService)).Export(member);
        }
        #endregion

        #region Dirty

        public static IEnumerable<string> GetDirtyUserProperties(this IContentBase entity)
        {
            return entity.Properties.Where(x => x.IsDirty()).Select(x => x.Alias);
        }

        public static bool IsAnyUserPropertyDirty(this IContentBase entity)
        {
            return entity.Properties.Any(x => x.IsDirty());
        }

        public static bool WasAnyUserPropertyDirty(this IContentBase entity)
        {
            return entity.Properties.Any(x => x.WasDirty());
        }

        #endregion
    }
}