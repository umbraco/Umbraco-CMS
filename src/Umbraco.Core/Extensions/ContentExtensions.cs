// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Xml.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Extensions;

public static class ContentExtensions
{
    /// <summary>
    ///     Returns the path to a media item stored in a property if the property editor is <see cref="IMediaUrlGenerator" />
    /// </summary>
    /// <param name="content"></param>
    /// <param name="propertyTypeAlias"></param>
    /// <param name="mediaUrlGenerators"></param>
    /// <param name="mediaFilePath"></param>
    /// <param name="culture"></param>
    /// <param name="segment"></param>
    /// <returns>True if the file path can be resolved and the property is <see cref="IMediaUrlGenerator" /></returns>
    public static bool TryGetMediaPath(
        this IContentBase content,
        string propertyTypeAlias,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        out string? mediaFilePath,
        string? culture = null,
        string? segment = null)
    {
        if (!content.Properties.TryGetValue(propertyTypeAlias, out IProperty? property))
        {
            mediaFilePath = null;
            return false;
        }

        if (!mediaUrlGenerators.TryGetMediaPath(
                property?.PropertyType?.PropertyEditorAlias,
                property?.GetValue(culture, segment),
                out mediaFilePath))
        {
            return false;
        }

        return true;
    }

    public static bool IsAnyUserPropertyDirty(this IContentBase entity) => entity.Properties.Any(x => x.IsDirty());

    public static bool WasAnyUserPropertyDirty(this IContentBase entity) => entity.Properties.Any(x => x.WasDirty());

    public static bool IsMoving(this IContentBase entity)
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

    /// <summary>
    ///     Removes characters that are not valid XML characters from all entity properties
    ///     of type string. See: http://stackoverflow.com/a/961504/5018
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     If this is not done then the xml cache can get corrupt and it will throw YSODs upon reading it.
    /// </remarks>
    /// <param name="entity"></param>
    public static void SanitizeEntityPropertiesForXmlStorage(this IContentBase entity)
    {
        entity.Name = entity.Name?.ToValidXmlString();
        foreach (IProperty property in entity.Properties)
        {
            foreach (IPropertyValue propertyValue in property.Values)
            {
                if (propertyValue.EditedValue is string editString)
                {
                    propertyValue.EditedValue = editString.ToValidXmlString();
                }

                if (propertyValue.PublishedValue is string publishedString)
                {
                    propertyValue.PublishedValue = publishedString.ToValidXmlString();
                }
            }
        }
    }

    /// <summary>
    ///     Returns all properties based on the editorAlias
    /// </summary>
    /// <param name="content"></param>
    /// <param name="editorAlias"></param>
    /// <returns></returns>
    public static IEnumerable<IProperty> GetPropertiesByEditor(this IContentBase content, string editorAlias)
        => content.Properties.Where(x => x.PropertyType?.PropertyEditorAlias == editorAlias);

    /// <summary>
    ///     Checks if the IContentBase has children
    /// </summary>
    /// <param name="content"></param>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This is a bit of a hack because we need to type check!
    /// </remarks>
    internal static bool? HasChildren(IContentBase content, ServiceContext services)
    {
        if (content is IContent)
        {
            return services.ContentService?.HasChildren(content.Id);
        }

        if (content is IMedia)
        {
            return services.MediaService?.HasChildren(content.Id);
        }

        return false;
    }

    /// <summary>
    ///     Gets the <see cref="IProfile" /> for the Creator of this content item.
    /// </summary>
    public static IProfile? GetCreatorProfile(this IContentBase content, IUserService userService) =>
        userService.GetProfileById(content.CreatorId);

    /// <summary>
    ///     Gets the <see cref="IProfile" /> for the Writer of this content.
    /// </summary>
    public static IProfile? GetWriterProfile(this IContent content, IUserService userService) =>
        userService.GetProfileById(content.WriterId);

    /// <summary>
    ///     Gets the <see cref="IProfile" /> for the Writer of this content.
    /// </summary>
    public static IProfile? GetWriterProfile(this IMedia content, IUserService userService) =>
        userService.GetProfileById(content.WriterId);

    #region User/Profile methods

    /// <summary>
    ///     Gets the <see cref="IProfile" /> for the Creator of this media item.
    /// </summary>
    public static IProfile? GetCreatorProfile(this IMedia media, IUserService userService) =>
        userService.GetProfileById(media.CreatorId);

    #endregion

    /// <summary>
    ///     Returns properties that do not belong to a group
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static IEnumerable<IProperty> GetNonGroupedProperties(this IContentBase content) =>
        content.Properties
            .Where(x => x.PropertyType?.PropertyGroupId == null)
            .OrderBy(x => x.PropertyType?.SortOrder);

    /// <summary>
    ///     Returns the Property object for the given property group
    /// </summary>
    /// <param name="content"></param>
    /// <param name="propertyGroup"></param>
    /// <returns></returns>
    public static IEnumerable<IProperty>
        GetPropertiesForGroup(this IContentBase content, PropertyGroup propertyGroup) =>

        // get the properties for the current tab
        content.Properties
            .Where(property => propertyGroup.PropertyTypes is not null && propertyGroup.PropertyTypes
                .Select(propertyType => propertyType.Id)
                .Contains(property.PropertyTypeId));

    #region Dirty

    public static IEnumerable<string> GetDirtyUserProperties(this IContentBase entity) =>
        entity.Properties.Where(x => x.IsDirty()).Select(x => x.Alias);

    #endregion

    /// <summary>
    ///     Creates the full xml representation for the <see cref="IContent" /> object and all of it's descendants
    /// </summary>
    /// <param name="content"><see cref="IContent" /> to generate xml for</param>
    /// <param name="serializer"></param>
    /// <returns>Xml representation of the passed in <see cref="IContent" /></returns>
    public static XElement ToDeepXml(this IContent content, IEntityXmlSerializer serializer) =>
        serializer.Serialize(content, false, true);

    /// <summary>
    ///     Creates the xml representation for the <see cref="IContent" /> object
    /// </summary>
    /// <param name="content"><see cref="IContent" /> to generate xml for</param>
    /// <param name="serializer"></param>
    /// <returns>Xml representation of the passed in <see cref="IContent" /></returns>
    public static XElement ToXml(this IContent content, IEntityXmlSerializer serializer) =>
        serializer.Serialize(content, false);

    /// <summary>
    ///     Creates the xml representation for the <see cref="IMedia" /> object
    /// </summary>
    /// <param name="media"><see cref="IContent" /> to generate xml for</param>
    /// <param name="serializer"></param>
    /// <returns>Xml representation of the passed in <see cref="IContent" /></returns>
    public static XElement ToXml(this IMedia media, IEntityXmlSerializer serializer) => serializer.Serialize(media);

    /// <summary>
    ///     Creates the xml representation for the <see cref="IMember" /> object
    /// </summary>
    /// <param name="member"><see cref="IMember" /> to generate xml for</param>
    /// <param name="serializer"></param>
    /// <returns>Xml representation of the passed in <see cref="IContent" /></returns>
    public static XElement ToXml(this IMember member, IEntityXmlSerializer serializer) => serializer.Serialize(member);

    #region IContent

    /// <summary>
    ///     Gets the current status of the Content
    /// </summary>
    public static ContentStatus GetStatus(this IContent content, ContentScheduleCollection contentSchedule, string? culture = null)
    {
        if (content.Trashed)
        {
            return ContentStatus.Trashed;
        }

        if (!content.ContentType.VariesByCulture())
        {
            culture = string.Empty;
        }
        else if (culture.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException($"{nameof(culture)} cannot be null or empty");
        }

        IEnumerable<ContentSchedule> expires = contentSchedule.GetSchedule(culture!, ContentScheduleAction.Expire);
        if (expires != null && expires.Any(x => x.Date > DateTime.MinValue && DateTime.Now > x.Date))
        {
            return ContentStatus.Expired;
        }

        IEnumerable<ContentSchedule> release = contentSchedule.GetSchedule(culture!, ContentScheduleAction.Release);
        if (release != null && release.Any(x => x.Date > DateTime.MinValue && x.Date > DateTime.Now))
        {
            return ContentStatus.AwaitingRelease;
        }

        if (content.Published)
        {
            return ContentStatus.Published;
        }

        return ContentStatus.Unpublished;
    }

    /// <summary>
    ///     Gets a collection containing the ids of all ancestors.
    /// </summary>
    /// <param name="content"><see cref="IContent" /> to retrieve ancestors for</param>
    /// <returns>An Enumerable list of integer ids</returns>
    public static IEnumerable<int>? GetAncestorIds(this IContent content) =>
        content.Path?.Split(Constants.CharArrays.Comma)
            .Where(x => x != Constants.System.RootString && x != content.Id.ToString(CultureInfo.InvariantCulture))
            .Select(s =>
                int.Parse(s, CultureInfo.InvariantCulture));

    #endregion

    #region SetValue for setting file contents

    /// <summary>
    ///     Sets the posted file value of a property.
    /// </summary>
    public static void SetValue(
        this IContentBase content,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IShortStringHelper shortStringHelper,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        string propertyTypeAlias,
        string filename,
        Stream filestream,
        string? culture = null,
        string? segment = null)
    {
        if (filename == null || filestream == null)
        {
            return;
        }

        filename = shortStringHelper.CleanStringForSafeFileName(filename);
        if (string.IsNullOrWhiteSpace(filename))
        {
            return;
        }

        filename = filename.ToLower();

        SetUploadFile(content, mediaFileManager, mediaUrlGenerators, contentTypeBaseServiceProvider, propertyTypeAlias, filename, filestream, culture, segment);
    }

    /// <summary>
    ///     Stores a file.
    /// </summary>
    /// <param name="content"><see cref="IContentBase" />A content item.</param>
    /// <param name="propertyTypeAlias">The property alias.</param>
    /// <param name="filename">The name of the file.</param>
    /// <param name="filestream">A stream containing the file data.</param>
    /// <param name="filepath">The original file path, if any.</param>
    /// <returns>The path to the file, relative to the media filesystem.</returns>
    /// <remarks>
    ///     <para>
    ///         Does NOT set the property value, so one should probably store the file and then do
    ///         something alike: property.Value = MediaHelper.FileSystem.GetUrl(filepath).
    ///     </para>
    ///     <para>
    ///         The original file path is used, in the old media file path scheme, to try and reuse
    ///         the "folder number" that was assigned to the previous file referenced by the property,
    ///         if any.
    ///     </para>
    /// </remarks>
    public static string StoreFile(
        this IContentBase content,
        MediaFileManager mediaFileManager,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        string propertyTypeAlias,
        string filename,
        Stream filestream,
        string filepath)
    {
        IContentTypeComposition? contentType = contentTypeBaseServiceProvider.GetContentTypeOf(content);
        IPropertyType? propertyType = contentType?
            .CompositionPropertyTypes.FirstOrDefault(x => x.Alias?.InvariantEquals(propertyTypeAlias) ?? false);
        if (propertyType == null)
        {
            throw new ArgumentException("Invalid property type alias " + propertyTypeAlias + ".");
        }

        return mediaFileManager.StoreFile(content, propertyType, filename, filestream, filepath);
    }

    private static void SetUploadFile(
        this IContentBase content,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        string propertyTypeAlias,
        string filename,
        Stream filestream,
        string? culture = null,
        string? segment = null)
    {
        IProperty property = GetProperty(content, contentTypeBaseServiceProvider, propertyTypeAlias);

        // Fixes https://github.com/umbraco/Umbraco-CMS/issues/3937 - Assigning a new file to an
        // existing IMedia with extension SetValue causes exception 'Illegal characters in path'
        string? oldpath = null;

        if (content.TryGetMediaPath(property.Alias, mediaUrlGenerators, out var mediaFilePath, culture, segment))
        {
            oldpath = mediaFileManager.FileSystem.GetRelativePath(mediaFilePath!);
        }

        var filepath = mediaFileManager.StoreFile(content, property.PropertyType, filename, filestream, oldpath);

        // NOTE: Here we are just setting the value to a string which means that any file based editor
        // will need to handle the raw string value and save it to it's correct (i.e. JSON)
        // format. I'm unsure how this works today with image cropper but it does (maybe events?)
        property.SetValue(mediaFileManager.FileSystem.GetUrl(filepath), culture, segment);
    }

    // gets or creates a property for a content item.
    private static IProperty GetProperty(
        IContentBase content,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        string propertyTypeAlias)
    {
        IProperty? property = content.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
        if (property != null)
        {
            return property;
        }

        IContentTypeComposition? contentType = contentTypeBaseServiceProvider.GetContentTypeOf(content);
        IPropertyType? propertyType = contentType?.CompositionPropertyTypes
            .FirstOrDefault(x => x.Alias?.InvariantEquals(propertyTypeAlias) ?? false);
        if (propertyType == null)
        {
            throw new Exception("No property type exists with alias " + propertyTypeAlias + ".");
        }

        property = new Property(propertyType);
        content.Properties.Add(property);
        return property;
    }

    #endregion
}
