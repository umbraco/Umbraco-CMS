using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
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
        /// Determines if the item should be persisted at all
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// In one particular case, a content item shouldn't be persisted:
        /// * The item exists and is published
        /// * A call to ContentService.Save is made
        /// * The item has not been modified whatsoever apart from changing it's published status from published to saved
        ///
        /// In this case, there is no reason to make any database changes at all
        /// </remarks>
        internal static bool RequiresSaving(this IContent entity)
        {
            var publishedState = ((Content)entity).PublishedState;
            return RequiresSaving(entity, publishedState);
        }

        /// <summary>
        /// Determines if the item should be persisted at all
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="publishedState"></param>
        /// <returns></returns>
        /// <remarks>
        /// In one particular case, a content item shouldn't be persisted:
        /// * The item exists and is published
        /// * A call to ContentService.Save is made
        /// * The item has not been modified whatsoever apart from changing it's published status from published to saved
        ///
        /// In this case, there is no reason to make any database changes at all
        /// </remarks>
        internal static bool RequiresSaving(this IContent entity, PublishedState publishedState)
        {
            var publishedChanged = entity.IsPropertyDirty("Published") && publishedState != PublishedState.Unpublished;
            //check if any user prop has changed
            var propertyValueChanged = entity.IsAnyUserPropertyDirty();

            //We need to know if any other property apart from Published was changed here
            //don't create a new version if the published state has changed to 'Save' but no data has actually been changed
            if (publishedChanged && entity.Published == false && propertyValueChanged == false)
            {
                //at this point we need to check if any non property value has changed that wasn't the published state
                var changedProps = ((TracksChangesEntityBase)entity).GetDirtyProperties();
                if (changedProps.Any(x => x != "Published") == false)
                {
                    return false;
                }
            }

            return true;
        }

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
        /// Returns a list of all dirty user defined properties
        /// </summary>
        /// <returns></returns>
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
            //check if the published state has changed or the language
            var publishedChanged = entity.IsPropertyDirty("Published") && publishedState != PublishedState.Unpublished;
            var langChanged = entity.IsPropertyDirty("Language");
            var contentChanged = publishedChanged || langChanged;

            //check if any user prop has changed
            var propertyValueChanged = entity.IsAnyUserPropertyDirty();

            //return true if published or language has changed
            if (contentChanged)
            {
                return true;
            }

            //check if any content prop has changed
            var contentDataChanged = ((Content)entity).IsEntityDirty();

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
            if (entity.IsPropertyDirty("Published") && (entity.Published || publishedState == PublishedState.Unpublished))
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
                    var value = (string) property.Value;
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
                                                          .Contains(property.PropertyTypeId));
        }

        /// <summary>
        /// Set property values by alias with an annonymous object
        /// </summary>
        public static void PropertyValues(this IContentBase content, object value)
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
            filename = filename.ToLower();

            FileSystemProviderManager.Current.MediaFileSystem.SetUploadFile(content, propertyTypeAlias, filename, value.InputStream);
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
            filename = filename.ToLower();

            FileSystemProviderManager.Current.MediaFileSystem.SetUploadFile(content, propertyTypeAlias, filename, filestream);
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
            return FileSystemProviderManager.Current.MediaFileSystem.StoreFile(content, propertyType, filename, filestream, filepath);
        }

        #endregion

        #region User/Profile methods

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Creator of this media item.
        /// </summary>
        [Obsolete("Use the overload that declares the IUserService to use")]
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

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Creator of this content item.
        /// </summary>
        [Obsolete("Use the overload that declares the IUserService to use")]
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

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Writer of this content.
        /// </summary>
        [Obsolete("Use the overload that declares the IUserService to use")]
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

        ///// <summary>
        ///// Returns the tags for the given property
        ///// </summary>
        ///// <param name="content"></param>
        ///// <param name="propertyTypeAlias"></param>
        ///// <param name="tagGroup"></param>
        ///// <returns></returns>
        ///// <remarks>
        ///// The tags returned are only relavent for published content & saved media or members
        ///// </remarks>
        //public static IEnumerable<ITag> GetTags(this IContentBase content, string propertyTypeAlias, string tagGroup = "default")
        //{

        //}

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

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
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

        /// <summary>
        /// Creates the xml representation for the <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media"><see cref="IContent"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
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

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <param name="isPreview">Boolean indicating whether the xml should be generated for preview</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
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

        /// <summary>
        /// Creates the xml representation for the <see cref="IMember"/> object
        /// </summary>
        /// <param name="member"><see cref="IMember"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
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
    }
}