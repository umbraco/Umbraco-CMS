using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Core
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Returns all properties based on the editorAlias
        /// </summary>
        /// <param name="content"></param>
        /// <param name="editorAlias"></param>
        /// <returns></returns>
        public static IEnumerable<IProperty> GetPropertiesByEditor(this IContentBase content, string editorAlias)
            => content.Properties.Where(x => x.PropertyType.PropertyEditorAlias == editorAlias);

        internal static bool IsMoving(this IContentBase entity)
        {
            // Check if this entity is being moved as a descendant as part of a bulk moving operations.
            // When this occurs, only Path + Level + UpdateDate are being changed. In this case we can bypass a lot of the below
            // operations which will make this whole operation go much faster. When moving we don't need to create
            // new versions, etc... because we cannot roll this operation back anyways.
            var isMoving = entity.IsPropertyDirty(nameof(entity.Path))
                           && entity.IsPropertyDirty(nameof(entity.Level))
                           && entity.IsPropertyDirty(nameof(entity.UpdateDate));

            return isMoving;
        }



        #region IContent

        /// <summary>
        /// Gets the current status of the Content
        /// </summary>
        public static ContentStatus GetStatus(this IContent content, string culture = null)
        {
            if (content.Trashed)
                return ContentStatus.Trashed;

            if (!content.ContentType.VariesByCulture())
                culture = string.Empty;
            else if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullException($"{nameof(culture)} cannot be null or empty");

            var expires = content.ContentSchedule.GetSchedule(culture, ContentScheduleAction.Expire);
            if (expires != null && expires.Any(x => x.Date > DateTime.MinValue && DateTime.Now > x.Date))
                return ContentStatus.Expired;

            var release = content.ContentSchedule.GetSchedule(culture, ContentScheduleAction.Release);
            if (release != null && release.Any(x => x.Date > DateTime.MinValue && x.Date > DateTime.Now))
                return ContentStatus.AwaitingRelease;

            if (content.Published)
                return ContentStatus.Published;

            return ContentStatus.Unpublished;
        }



        #endregion

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

        #region User/Profile methods

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Creator of this media item.
        /// </summary>
        public static IProfile GetCreatorProfile(this IMedia media, IUserService userService)
        {
            return userService.GetProfileById(media.CreatorId);
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
        /// Returns properties that do not belong to a group
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IEnumerable<IProperty> GetNonGroupedProperties(this IContentBase content)
        {
            return content.Properties
                .Where(x => x.PropertyType.PropertyGroupId == null)
                .OrderBy(x => x.PropertyType.SortOrder);
        }

        /// <summary>
        /// Returns the Property object for the given property group
        /// </summary>
        /// <param name="content"></param>
        /// <param name="propertyGroup"></param>
        /// <returns></returns>
        public static IEnumerable<IProperty> GetPropertiesForGroup(this IContentBase content, PropertyGroup propertyGroup)
        {
            //get the properties for the current tab
            return content.Properties
                .Where(property => propertyGroup.PropertyTypes
                    .Select(propertyType => propertyType.Id)
                    .Contains(property.PropertyTypeId));
        }


        #region SetValue for setting file contents

        /// <summary>
        /// Sets the posted file value of a property.
        /// </summary>
        public static void SetValue(this IContentBase content, IMediaFileSystem mediaFileSystem, IShortStringHelper shortStringHelper, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, string propertyTypeAlias, string filename, Stream filestream, string culture = null, string segment = null)
        {
            if (filename == null || filestream == null) return;

            filename = shortStringHelper.CleanStringForSafeFileName(filename);
            if (string.IsNullOrWhiteSpace(filename)) return;
            filename = filename.ToLower();

            SetUploadFile(content, mediaFileSystem, contentTypeBaseServiceProvider, propertyTypeAlias, filename, filestream, culture, segment);
        }

        private static void SetUploadFile(this IContentBase content, IMediaFileSystem mediaFileSystem, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, string propertyTypeAlias, string filename, Stream filestream, string culture = null, string segment = null)
        {
            var property = GetProperty(content, contentTypeBaseServiceProvider, propertyTypeAlias);

            // Fixes https://github.com/umbraco/Umbraco-CMS/issues/3937 - Assigning a new file to an
            // existing IMedia with extension SetValue causes exception 'Illegal characters in path'
            string oldpath = null;
            if (property.GetValue(culture, segment) is string svalue)
            {
                if (svalue.DetectIsJson())
                {
                    // the property value is a JSON serialized image crop data set - grab the "src" property as the file source
                    var jObject = JsonConvert.DeserializeObject<JObject>(svalue);
                    svalue = jObject != null ? jObject.GetValueAsString("src") : svalue;
                }
                oldpath = mediaFileSystem.GetRelativePath(svalue);
            }

            var filepath = mediaFileSystem.StoreFile(content, property.PropertyType, filename, filestream, oldpath);
            property.SetValue(mediaFileSystem.GetUrl(filepath), culture, segment);
        }

        // gets or creates a property for a content item.
        private static IProperty GetProperty(IContentBase content, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, string propertyTypeAlias)
        {
            var property = content.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            if (property != null) return property;

            var contentType = contentTypeBaseServiceProvider.GetContentTypeOf(content);
            var propertyType = contentType.CompositionPropertyTypes
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
        public static string StoreFile(this IContentBase content, IMediaFileSystem mediaFileSystem, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, string propertyTypeAlias, string filename, Stream filestream, string filepath)
        {
            var contentType = contentTypeBaseServiceProvider.GetContentTypeOf(content);
            var propertyType = contentType
                .CompositionPropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            if (propertyType == null) throw new ArgumentException("Invalid property type alias " + propertyTypeAlias + ".");
            return mediaFileSystem.StoreFile(content, propertyType, filename, filestream, filepath);
        }

        #endregion


        #region Dirty

        public static IEnumerable<string> GetDirtyUserProperties(this IContentBase entity)
        {
            return entity.Properties.Where(x => x.IsDirty()).Select(x => x.Alias);
        }



        #endregion
    }
}
