using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    public static class ContentExtensions
    {
        // this ain't pretty
        private static MediaFileSystem _mediaFileSystem;
        private static MediaFileSystem MediaFileSystem => _mediaFileSystem ?? (_mediaFileSystem = Current.FileSystems.MediaFileSystem);

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
            return Current.Services.ContentService.GetAncestors(content);
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
            return Current.Services.ContentService.GetChildren(content.Id);
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
            return Current.Services.ContentService.GetDescendants(content);
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
            return Current.Services.ContentService.GetById(content.ParentId);
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
            return Current.Services.MediaService.GetAncestors(media);
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
            return Current.Services.MediaService.GetChildren(media.Id);
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
            return Current.Services.MediaService.GetDescendants(media);
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
            return Current.Services.MediaService.GetById(media.ParentId);
        }
        #endregion

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
                foreach (var propertyValue in property.Values)
                {
                    if (propertyValue.EditValue is string editString)
                        propertyValue.EditValue = editString.ToValidXmlString();
                    if (propertyValue.PublishedValue is string publishedString)
                        propertyValue.PublishedValue = publishedString.ToValidXmlString();
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
                                                          .Contains(property.PropertyTypeId));
        }

        /// <summary>
        /// Set property values by alias with an annonymous object
        /// </summary>
        public static void PropertyValues(this IContentBase content, object value) // fixme kill that one! won't work with variants
        {
            if (value == null)
                throw new Exception("No properties has been passed in");

            var propertyInfos = value.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                //Check if a PropertyType with alias exists thus being a valid property
                var propertyType = content.PropertyTypes.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                if (propertyType == null)
                    throw new Exception($"The property alias {propertyInfo.Name} is not valid, because no PropertyType with this alias exists");

                //Check if a Property with the alias already exists in the collection thus being updated or inserted
                var item = content.Properties.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                if (item != null)
                {
                    item.SetValue(propertyInfo.GetValue(value, null));
                    //Update item with newly added value
                    content.Properties.Add(item);
                }
                else
                {
                    //Create new Property to add to collection
                    var property = propertyType.CreateProperty();
                    property.SetValue(propertyInfo.GetValue(value, null));
                    content.Properties.Add(property);
                }
            }
        }

        public static IContentTypeComposition GetContentType(this IContentBase contentBase)
        {
            if (contentBase == null) throw new ArgumentNullException("contentBase");

            var content = contentBase as IContent;
            if (content != null) return content.ContentType;
            var media = contentBase as IMedia;
            if (media != null) return media.ContentType;
            var member = contentBase as IMember;
            if (member != null) return member.ContentType;
            throw new NotSupportedException("Unsupported IContentBase implementation: " + contentBase.GetType().FullName + ".");
        }

        #region SetValue for setting file contents

        /// <summary>
        /// Stores and sets an uploaded HttpPostedFileBase as a property value.
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/>A content item.</param>
        /// <param name="propertyTypeAlias">The property alias.</param>
        /// <param name="value">The uploaded <see cref="HttpPostedFileBase"/>.</param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFileBase value)
        {
            // ensure we get the filename without the path in IE in intranet mode
            // http://stackoverflow.com/questions/382464/httppostedfile-filename-different-from-ie
            var filename = value.FileName;
            var pos = filename.LastIndexOf(@"\", StringComparison.InvariantCulture);
            if (pos > 0)
                filename = filename.Substring(pos + 1);

            // strip any directory info
            pos = filename.LastIndexOf(IOHelper.DirSepChar);
            if (pos > 0)
                filename = filename.Substring(pos + 1);

            // get a safe & clean filename
            filename = IOHelper.SafeFileName(filename);
            if (string.IsNullOrWhiteSpace(filename)) return;
            filename = filename.ToLower(); // fixme - er... why?

            MediaFileSystem.SetUploadFile(content, propertyTypeAlias, filename, value.InputStream);
        }

        /// <summary>
        /// Stores and sets an uploaded HttpPostedFile as a property value.
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/>A content item.</param>
        /// <param name="propertyTypeAlias">The property alias.</param>
        /// <param name="value">The uploaded <see cref="HttpPostedFile"/>.</param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFile value)
        {
            SetValue(content, propertyTypeAlias, (HttpPostedFileBase) new HttpPostedFileWrapper(value));
        }

        /// <summary>
        /// Stores and sets an uploaded HttpPostedFileWrapper as a property value.
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/>A content item.</param>
        /// <param name="propertyTypeAlias">The property alias.</param>
        /// <param name="value">The uploaded <see cref="HttpPostedFileWrapper"/>.</param>
        [Obsolete("There is no reason for this overload since HttpPostedFileWrapper inherits from HttpPostedFileBase")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFileWrapper value)
        {
            SetValue(content, propertyTypeAlias, (HttpPostedFileBase) value);
        }

        /// <summary>
        /// Stores and sets a file as a property value.
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/>A content item.</param>
        /// <param name="propertyTypeAlias">The property alias.</param>
        /// <param name="filename">The name of the file.</param>
        /// <param name="filestream">A stream containing the file data.</param>
        /// <remarks>This really is for FileUpload fields only, and should be obsoleted. For anything else,
        /// you need to store the file by yourself using Store and then figure out
        /// how to deal with auto-fill properties (if any) and thumbnails (if any) by yourself.</remarks>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, string filename, Stream filestream)
        {
            if (filename == null || filestream == null) return;

            // get a safe & clean filename
            filename = IOHelper.SafeFileName(filename);
            if (string.IsNullOrWhiteSpace(filename)) return;
            filename = filename.ToLower(); // fixme - er... why?

            MediaFileSystem.SetUploadFile(content, propertyTypeAlias, filename, filestream);
        }

        /// <summary>
        /// Stores a file.
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/>A content item.</param>
        /// <param name="propertyTypeAlias">The property alias.</param>
        /// <param name="filename">The name of the file.</param>
        /// <param name="filestream">A stream containing the file data.</param>
        /// <param name="filepath">The original file path, if any.</param>
        /// <returns>The path to the file, relative to the media filesystem.</returns>
        /// <remarks>
        /// <para>Does NOT set the property value, so one should probably store the file and then do
        /// something alike: property.Value = MediaHelper.FileSystem.GetUrl(filepath).</para>
        /// <para>The original file path is used, in the old media file path scheme, to try and reuse
        /// the "folder number" that was assigned to the previous file referenced by the property,
        /// if any.</para>
        /// </remarks>
        public static string StoreFile(this IContentBase content, string propertyTypeAlias, string filename, Stream filestream, string filepath)
        {
            var propertyType = content.GetContentType()
                .CompositionPropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            if (propertyType == null) throw new ArgumentException("Invalid property type alias " + propertyTypeAlias + ".");
            return MediaFileSystem.StoreFile(content, propertyType, filename, filestream, filepath);
        }

        #endregion

        #region User/Profile methods


        [Obsolete("Use the overload that declares the IUserService to use")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IProfile GetCreatorProfile(this IMedia media)
        {
            return Current.Services.UserService.GetProfileById(media.CreatorId);
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
            return Current.Services.UserService.GetProfileById(content.CreatorId);
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
            return Current.Services.UserService.GetProfileById(content.WriterId);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Writer of this content.
        /// </summary>
        public static IProfile GetWriterProfile(this IContent content, IUserService userService)
        {
            return userService.GetProfileById(content.WriterId);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Writer of this content.
        /// </summary>
        public static IProfile GetWriterProfile(this IMedia content, IUserService userService)
        {
            return userService.GetProfileById(content.WriterId);
        }

        #endregion

        #region Tags

        /// <summary>
        /// Sets tags.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property alias.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="replaceTags">True to replace the tags with the specified tags or false to merge them with the currently assigned ones.</param>
        /// <param name="tagGroup">The tags group.</param>
        /// <param name="storage">The tags storage type.</param>
        public static void SetTags(this IContentBase content, string propertyTypeAlias, IEnumerable<string> tags, bool replaceTags = true, string tagGroup = "default", TagCacheStorageType storage = TagCacheStorageType.Csv)
        {
            var property = content.Properties[propertyTypeAlias];
            if (property == null)
                throw new IndexOutOfRangeException("No property exists with name " + propertyTypeAlias);
            property.SetTags(propertyTypeAlias, tags, replaceTags, tagGroup, storage);
        }

        // fixme - totally not ok with variants
        internal static void SetTags(this Property property, string propertyTypeAlias, IEnumerable<string> tags, bool replaceTags = true, string tagGroup = "default", TagCacheStorageType storage = TagCacheStorageType.Csv)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var trimmedTags = tags.Select(x => x.Trim()).ToArray();
            var changes = property.TagChanges;

            changes.Add(new PropertyTagChange
            {
                Type = replaceTags ? PropertyTagChange.ChangeType.Replace : PropertyTagChange.ChangeType.Merge,
                Tags = trimmedTags.Select(x => new Tuple<string, string>(x, tagGroup))
            });

            // ensure the property value is set to the same thing
            if (replaceTags)
            {
                switch (storage)
                {
                    case TagCacheStorageType.Csv:
                        property.SetValue(string.Join(",", trimmedTags)); // csv string
                        break;
                    case TagCacheStorageType.Json:
                        property.SetValue(JsonConvert.SerializeObject(trimmedTags)); // json array
                        break;
                }
            }
            else // merge
            {
                IEnumerable<string> currentTags;
                switch (storage)
                {
                    case TagCacheStorageType.Csv:
                        currentTags = property.GetValue().ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                        property.SetValue(string.Join(",", currentTags.Union(trimmedTags))); // csv string
                        break;
                    case TagCacheStorageType.Json:
                        currentTags = JsonConvert.DeserializeObject<JArray>(property.GetValue().ToString()).Select(x => x.ToString());
                        property.SetValue(JsonConvert.SerializeObject(currentTags.Union(trimmedTags).ToArray())); // json array
                        break;
                }
            }
        }

        /// <summary>
        /// Remove tags.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property alias.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="tagGroup">The tags group.</param>
        // fixme - totally not ok with variants
        public static void RemoveTags(this IContentBase content, string propertyTypeAlias, IEnumerable<string> tags, string tagGroup = "default")
        {
            var property = content.Properties[propertyTypeAlias];
            if (property == null)
                throw new IndexOutOfRangeException("No property exists with name " + propertyTypeAlias);

            var trimmedTags = tags.Select(x => x.Trim()).ToArray();
            var changes = property.TagChanges;

            changes.Add(new PropertyTagChange
            {
                Type = PropertyTagChange.ChangeType.Remove,
                Tags = trimmedTags.Select(x => new Tuple<string, string>(x, tagGroup))
            });

            // set the property value
            var value = property.GetValue()?.ToString();
            if (string.IsNullOrWhiteSpace(value)) return;

            var storage = value.StartsWith("[") ? TagCacheStorageType.Json : TagCacheStorageType.Csv;
            IEnumerable<string> currentTags;
            switch (storage)
            {
                case TagCacheStorageType.Csv:
                    currentTags = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                    property.SetValue(string.Join(",", currentTags.Except(trimmedTags)));
                    break;
                case TagCacheStorageType.Json:
                    currentTags = JsonConvert.DeserializeObject<JArray>(property.GetValue().ToString()).Select(x => x.ToString());
                    property.SetValue(JsonConvert.SerializeObject(currentTags.Except(trimmedTags).ToArray())); // json array
                    break;
            }
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
            return Current.Services.PackagingService.Export(content, raiseEvents: false);
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
            return Current.Services.PackagingService.Export(media, raiseEvents: false);
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
            return ((PackagingService)(Current.Services.PackagingService)).Export(member);
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
