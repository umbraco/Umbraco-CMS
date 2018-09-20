﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core
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
                    if (propertyValue.EditedValue is string editString)
                        propertyValue.EditedValue = editString.ToValidXmlString();
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

        public static IContentTypeComposition GetContentType(this IContentBase contentBase)
        {
            if (contentBase == null) throw new ArgumentNullException(nameof(contentBase));

            if (contentBase is IContent content) return content.ContentType;
            if (contentBase is IMedia media) return media.ContentType;
            if (contentBase is IMember member) return member.ContentType;
            throw new NotSupportedException("Unsupported IContentBase implementation: " + contentBase.GetType().FullName + ".");
        }

        #region SetValue for setting file contents

        /// <summary>
        /// Sets the posted file value of a property.
        /// </summary>
        /// <remarks>This really is for FileUpload fields only, and should be obsoleted. For anything else,
        /// you need to store the file by yourself using Store and then figure out
        /// how to deal with auto-fill properties (if any) and thumbnails (if any) by yourself.</remarks>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, string filename, Stream filestream, string culture = null, string segment = null)
        {
            if (filename == null || filestream == null) return;

            // get a safe & clean filename
            filename = IOHelper.SafeFileName(filename);
            if (string.IsNullOrWhiteSpace(filename)) return;
            filename = filename.ToLower(); // fixme - er... why?

            SetUploadFile(content, propertyTypeAlias, filename, filestream, culture, segment);
        }

        private static void SetUploadFile(this IContentBase content, string propertyTypeAlias, string filename, Stream filestream, string culture = null, string segment = null)
        {
            var property = GetProperty(content, propertyTypeAlias);
            var oldpath = property.GetValue(culture, segment) is string svalue ? MediaFileSystem.GetRelativePath(svalue) : null;
            var filepath = MediaFileSystem.StoreFile(content, property.PropertyType, filename, filestream, oldpath);
            property.SetValue(MediaFileSystem.GetUrl(filepath), culture, segment);
        }

        // gets or creates a property for a content item.
        private static Property GetProperty(IContentBase content, string propertyTypeAlias)
        {
            var property = content.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            if (property != null) return property;

            var propertyType = content.GetContentType().CompositionPropertyTypes
                .FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            if (propertyType == null)
                throw new Exception("No property type exists with alias " + propertyTypeAlias + ".");

            property = new Property(propertyType);
            content.Properties.Add(property);
            return property;
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
