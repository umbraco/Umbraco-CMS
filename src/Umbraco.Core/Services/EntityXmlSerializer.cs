using System.Globalization;
using System.Net;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Serializes entities to XML
/// </summary>
internal class EntityXmlSerializer : IEntityXmlSerializer
{
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly ILocalizationService _localizationService;
    private readonly IMediaService _mediaService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly UrlSegmentProviderCollection _urlSegmentProviders;
    private readonly IUserService _userService;

    public EntityXmlSerializer(
        IContentService contentService,
        IMediaService mediaService,
        IDataTypeService dataTypeService,
        IUserService userService,
        ILocalizationService localizationService,
        IContentTypeService contentTypeService,
        UrlSegmentProviderCollection urlSegmentProviders,
        IShortStringHelper shortStringHelper,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
    {
        _contentTypeService = contentTypeService;
        _mediaService = mediaService;
        _contentService = contentService;
        _dataTypeService = dataTypeService;
        _userService = userService;
        _localizationService = localizationService;
        _urlSegmentProviders = urlSegmentProviders;
        _shortStringHelper = shortStringHelper;
        _propertyEditors = propertyEditors;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
    }

    /// <summary>
    ///     Exports an IContent item as an XElement.
    /// </summary>
    public XElement Serialize(
        IContent content,
        bool published,
        bool withDescendants = false) // TODO: take care of usage! only used for the packager
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        var nodeName = content.ContentType.Alias.ToSafeAlias(_shortStringHelper);

        XElement xml = SerializeContentBase(content, content.GetUrlSegment(_shortStringHelper, _urlSegmentProviders), nodeName, published);

        xml.Add(new XAttribute("nodeType", content.ContentType.Id));
        xml.Add(new XAttribute("nodeTypeAlias", content.ContentType.Alias));

        xml.Add(new XAttribute("creatorName", content.GetCreatorProfile(_userService)?.Name ?? "??"));

        // xml.Add(new XAttribute("creatorID", content.CreatorId));
        xml.Add(new XAttribute("writerName", content.GetWriterProfile(_userService)?.Name ?? "??"));
        xml.Add(new XAttribute("writerID", content.WriterId));

        xml.Add(new XAttribute("template", content.TemplateId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty));

        xml.Add(new XAttribute("isPublished", content.Published));

        if (withDescendants)
        {
            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                IEnumerable<IContent> children =
                    _contentService.GetPagedChildren(content.Id, page++, pageSize, out total);
                SerializeChildren(children, xml, published);
            }
        }

        return xml;
    }

    /// <summary>
    ///     Exports an IMedia item as an XElement.
    /// </summary>
    public XElement Serialize(
        IMedia media,
        bool withDescendants = false,
        Action<IMedia, XElement>? onMediaItemSerialized = null)
    {
        if (_mediaService == null)
        {
            throw new ArgumentNullException(nameof(_mediaService));
        }

        if (_dataTypeService == null)
        {
            throw new ArgumentNullException(nameof(_dataTypeService));
        }

        if (_userService == null)
        {
            throw new ArgumentNullException(nameof(_userService));
        }

        if (_localizationService == null)
        {
            throw new ArgumentNullException(nameof(_localizationService));
        }

        if (media == null)
        {
            throw new ArgumentNullException(nameof(media));
        }

        if (_urlSegmentProviders == null)
        {
            throw new ArgumentNullException(nameof(_urlSegmentProviders));
        }

        var nodeName = media.ContentType.Alias.ToSafeAlias(_shortStringHelper);

        const bool published = false; // always false for media
        var urlValue = media.GetUrlSegment(_shortStringHelper, _urlSegmentProviders);
        XElement xml = SerializeContentBase(media, urlValue, nodeName, published);

        xml.Add(new XAttribute("nodeType", media.ContentType.Id));
        xml.Add(new XAttribute("nodeTypeAlias", media.ContentType.Alias));

        // xml.Add(new XAttribute("creatorName", media.GetCreatorProfile(userService).Name));
        // xml.Add(new XAttribute("creatorID", media.CreatorId));
        xml.Add(new XAttribute("writerName", media.GetWriterProfile(_userService)?.Name ?? string.Empty));
        xml.Add(new XAttribute("writerID", media.WriterId));
        xml.Add(new XAttribute("udi", media.GetUdi()));

        // xml.Add(new XAttribute("template", 0)); // no template for media
        onMediaItemSerialized?.Invoke(media, xml);

        if (withDescendants)
        {
            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                IEnumerable<IMedia> children = _mediaService.GetPagedChildren(media.Id, page++, pageSize, out total);
                SerializeChildren(children, xml, onMediaItemSerialized);
            }
        }

        return xml;
    }

    /// <summary>
    ///     Exports an IMember item as an XElement.
    /// </summary>
    public XElement Serialize(IMember member)
    {
        var nodeName = member.ContentType.Alias.ToSafeAlias(_shortStringHelper);

        const bool published = false; // always false for member
        XElement xml = SerializeContentBase(member, string.Empty, nodeName, published);

        xml.Add(new XAttribute("nodeType", member.ContentType.Id));
        xml.Add(new XAttribute("nodeTypeAlias", member.ContentType.Alias));

        // what about writer/creator/version?
        xml.Add(new XAttribute("loginName", member.Username));
        xml.Add(new XAttribute("email", member.Email));
        xml.Add(new XAttribute("icon", member.ContentType.Icon!));

        return xml;
    }

    /// <summary>
    ///     Exports a list of Data Types
    /// </summary>
    /// <param name="dataTypeDefinitions">List of data types to export</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the IDataTypeDefinition objects</returns>
    public XElement Serialize(IEnumerable<IDataType> dataTypeDefinitions)
    {
        var container = new XElement("DataTypes");
        foreach (IDataType dataTypeDefinition in dataTypeDefinitions)
        {
            container.Add(Serialize(dataTypeDefinition));
        }

        return container;
    }

    public XElement Serialize(IDataType dataType)
    {
        var xml = new XElement("DataType");
        xml.Add(new XAttribute("Name", dataType.Name!));

        // The 'ID' when exporting is actually the property editor alias (in pre v7 it was the IDataType GUID id)
        xml.Add(new XAttribute("Id", dataType.EditorAlias));
        xml.Add(new XAttribute("Definition", dataType.Key));
        xml.Add(new XAttribute("DatabaseType", dataType.DatabaseType.ToString()));
        xml.Add(new XAttribute("Configuration", _configurationEditorJsonSerializer.Serialize(dataType.Configuration)));

        var folderNames = string.Empty;
        var folderKeys = string.Empty;
        if (dataType.Level != 1)
        {
            // get URL encoded folder names
            IOrderedEnumerable<EntityContainer> folders = _dataTypeService.GetContainers(dataType)
                .OrderBy(x => x.Level);

            folderNames = string.Join("/", folders.Select(x => WebUtility.UrlEncode(x.Name)).ToArray());
            folderKeys = string.Join("/", folders.Select(x => x.Key).ToArray());
        }

        if (string.IsNullOrWhiteSpace(folderNames) == false)
        {
            xml.Add(new XAttribute("Folders", folderNames));
            xml.Add(new XAttribute("FolderKeys", folderKeys));
        }

        return xml;
    }

    /// <summary>
    ///     Exports a list of <see cref="IDictionaryItem" /> items to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="dictionaryItem">List of dictionary items to export</param>
    /// <param name="includeChildren">Optional boolean indicating whether or not to include children</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the IDictionaryItem objects</returns>
    public XElement Serialize(IEnumerable<IDictionaryItem> dictionaryItem, bool includeChildren = true)
    {
        var xml = new XElement("DictionaryItems");
        foreach (IDictionaryItem item in dictionaryItem)
        {
            xml.Add(Serialize(item, includeChildren));
        }

        return xml;
    }

    /// <summary>
    ///     Exports a single <see cref="IDictionaryItem" /> item to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="dictionaryItem">Dictionary Item to export</param>
    /// <param name="includeChildren">Optional boolean indicating whether or not to include children</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the IDictionaryItem object</returns>
    public XElement Serialize(IDictionaryItem dictionaryItem, bool includeChildren)
    {
        XElement xml = Serialize(dictionaryItem);

        if (includeChildren)
        {
            IEnumerable<IDictionaryItem>? children = _localizationService.GetDictionaryItemChildren(dictionaryItem.Key);
            if (children is not null)
            {
                foreach (IDictionaryItem child in children)
                {
                    xml.Add(Serialize(child, true));
                }
            }
        }

        return xml;
    }

    public XElement Serialize(IStylesheet stylesheet, bool includeProperties)
    {
        var xml = new XElement(
            "Stylesheet",
            new XElement("Name", stylesheet.Alias),
            new XElement("FileName", stylesheet.Path),
            new XElement("Content", new XCData(stylesheet.Content!)));

        if (!includeProperties)
        {
            return xml;
        }

        var props = new XElement("Properties");
        xml.Add(props);

        if (stylesheet.Properties is not null)
        {
            foreach (IStylesheetProperty prop in stylesheet.Properties)
            {
                props.Add(new XElement(
                    "Property",
                    new XElement("Name", prop.Name),
                    new XElement("Alias", prop.Alias),
                    new XElement("Value", prop.Value)));
            }
        }

        return xml;
    }

    /// <summary>
    ///     Exports a list of <see cref="ILanguage" /> items to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="languages">List of Languages to export</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the ILanguage objects</returns>
    public XElement Serialize(IEnumerable<ILanguage> languages)
    {
        var xml = new XElement("Languages");
        foreach (ILanguage language in languages)
        {
            xml.Add(Serialize(language));
        }

        return xml;
    }

    public XElement Serialize(ILanguage language)
    {
        var xml = new XElement(
            "Language",
            new XAttribute("Id", language.Id),
            new XAttribute("CultureAlias", language.IsoCode),
            new XAttribute("FriendlyName", language.CultureName));

        return xml;
    }

    public XElement Serialize(ITemplate template)
    {
        var xml = new XElement("Template");
        xml.Add(new XElement("Name", template.Name));
        xml.Add(new XElement("Key", template.Key));
        xml.Add(new XElement("Alias", template.Alias));
        xml.Add(new XElement("Design", new XCData(template.Content!)));

        if (template is Template concreteTemplate && concreteTemplate.MasterTemplateId != null)
        {
            if (concreteTemplate.MasterTemplateId.IsValueCreated &&
                concreteTemplate.MasterTemplateId.Value != default)
            {
                xml.Add(new XElement("Master", concreteTemplate.MasterTemplateId.ToString()));
                xml.Add(new XElement("MasterAlias", concreteTemplate.MasterTemplateAlias));
            }
        }

        return xml;
    }

    /// <summary>
    ///     Exports a list of <see cref="ITemplate" /> items to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="templates"></param>
    /// <returns></returns>
    public XElement Serialize(IEnumerable<ITemplate> templates)
    {
        var xml = new XElement("Templates");
        foreach (ITemplate item in templates)
        {
            xml.Add(Serialize(item));
        }

        return xml;
    }

    public XElement Serialize(IMediaType mediaType)
    {
        var info = new XElement(
            "Info",
            new XElement("Name", mediaType.Name),
            new XElement("Alias", mediaType.Alias),
            new XElement("Key", mediaType.Key),
            new XElement("Icon", mediaType.Icon),
            new XElement("Thumbnail", mediaType.Thumbnail),
            new XElement("Description", mediaType.Description),
            new XElement("AllowAtRoot", mediaType.AllowedAsRoot.ToString()));

        var masterContentType = mediaType.CompositionAliases().FirstOrDefault();
        if (masterContentType != null)
        {
            info.Add(new XElement("Master", masterContentType));
        }

        var structure = new XElement("Structure");
        if (mediaType.AllowedContentTypes is not null)
        {
            foreach (ContentTypeSort allowedType in mediaType.AllowedContentTypes)
            {
                structure.Add(new XElement("MediaType", allowedType.Alias));
            }
        }

        var genericProperties = new XElement(
            "GenericProperties",
            SerializePropertyTypes(mediaType.PropertyTypes, mediaType.PropertyGroups)); // actually, all of them

        var tabs = new XElement(
            "Tabs",
            SerializePropertyGroups(mediaType.PropertyGroups)); // TODO Rename to PropertyGroups

        var xml = new XElement(
            "MediaType",
            info,
            structure,
            genericProperties,
            tabs);

        return xml;
    }

    /// <summary>
    ///     Exports a list of <see cref="IMacro" /> items to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="macros">Macros to export</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the IMacro objects</returns>
    public XElement Serialize(IEnumerable<IMacro> macros)
    {
        var xml = new XElement("Macros");
        foreach (IMacro item in macros)
        {
            xml.Add(Serialize(item));
        }

        return xml;
    }

    public XElement Serialize(IMacro macro)
    {
        var xml = new XElement("macro");
        xml.Add(new XElement("name", macro.Name));
        xml.Add(new XElement("key", macro.Key));
        xml.Add(new XElement("alias", macro.Alias));
        xml.Add(new XElement("macroSource", macro.MacroSource));
        xml.Add(new XElement("useInEditor", macro.UseInEditor.ToString()));
        xml.Add(new XElement("dontRender", macro.DontRender.ToString()));
        xml.Add(new XElement("refreshRate", macro.CacheDuration.ToString(CultureInfo.InvariantCulture)));
        xml.Add(new XElement("cacheByMember", macro.CacheByMember.ToString()));
        xml.Add(new XElement("cacheByPage", macro.CacheByPage.ToString()));

        var properties = new XElement("properties");
        foreach (IMacroProperty property in macro.Properties)
        {
            properties.Add(new XElement(
                "property",
                new XAttribute("key", property.Key),
                new XAttribute("name", property.Name!),
                new XAttribute("alias", property.Alias),
                new XAttribute("sortOrder", property.SortOrder),
                new XAttribute("propertyType", property.EditorAlias)));
        }

        xml.Add(properties);

        return xml;
    }

    public XElement Serialize(IContentType contentType)
    {
        var info = new XElement(
            "Info",
            new XElement("Name", contentType.Name),
            new XElement("Alias", contentType.Alias),
            new XElement("Key", contentType.Key),
            new XElement("Icon", contentType.Icon),
            new XElement("Thumbnail", contentType.Thumbnail),
            new XElement("Description", contentType.Description),
            new XElement("AllowAtRoot", contentType.AllowedAsRoot.ToString()),
            new XElement("IsListView", contentType.IsContainer.ToString()),
            new XElement("IsElement", contentType.IsElement.ToString()),
            new XElement("Variations", contentType.Variations.ToString()));

        IContentTypeComposition? masterContentType =
            contentType.ContentTypeComposition.FirstOrDefault(x => x.Id == contentType.ParentId);
        if (masterContentType != null)
        {
            info.Add(new XElement("Master", masterContentType.Alias));
        }

        var compositionsElement = new XElement("Compositions");
        IEnumerable<IContentTypeComposition> compositions = contentType.ContentTypeComposition;
        foreach (IContentTypeComposition composition in compositions)
        {
            compositionsElement.Add(new XElement("Composition", composition.Alias));
        }

        info.Add(compositionsElement);

        var allowedTemplates = new XElement("AllowedTemplates");
        if (contentType.AllowedTemplates is not null)
        {
            foreach (ITemplate template in contentType.AllowedTemplates)
            {
                allowedTemplates.Add(new XElement("Template", template.Alias));
            }
        }

        info.Add(allowedTemplates);

        if (contentType.DefaultTemplate != null && contentType.DefaultTemplate.Id != 0)
        {
            info.Add(new XElement("DefaultTemplate", contentType.DefaultTemplate.Alias));
        }
        else
        {
            info.Add(new XElement("DefaultTemplate", string.Empty));
        }

        var structure = new XElement("Structure");
        if (contentType.AllowedContentTypes is not null)
        {
            foreach (ContentTypeSort allowedType in contentType.AllowedContentTypes)
            {
                structure.Add(new XElement("DocumentType", allowedType.Alias));
            }
        }

        var genericProperties = new XElement(
            "GenericProperties",
            SerializePropertyTypes(contentType.PropertyTypes, contentType.PropertyGroups)); // actually, all of them

        var tabs = new XElement(
            "Tabs",
            SerializePropertyGroups(contentType.PropertyGroups)); // TODO Rename to PropertyGroups

        var xml = new XElement(
            "DocumentType",
            info,
            structure,
            genericProperties,
            tabs);

        if (contentType is IContentTypeWithHistoryCleanup withCleanup && withCleanup.HistoryCleanup is not null)
        {
            xml.Add(SerializeCleanupPolicy(withCleanup.HistoryCleanup));
        }

        var folderNames = string.Empty;
        var folderKeys = string.Empty;

        // don't add folders if this is a child doc type
        if (contentType.Level != 1 && masterContentType == null)
        {
            // get URL encoded folder names
            IOrderedEnumerable<EntityContainer> folders = _contentTypeService.GetContainers(contentType)
                .OrderBy(x => x.Level);

            folderNames = string.Join("/", folders.Select(x => WebUtility.UrlEncode(x.Name)).ToArray());
            folderKeys = string.Join("/", folders.Select(x => x.Key).ToArray());
        }

        if (string.IsNullOrWhiteSpace(folderNames) == false)
        {
            xml.Add(new XAttribute("Folders", folderNames));
            xml.Add(new XAttribute("FolderKeys", folderKeys));
        }

        return xml;
    }

    private XElement Serialize(IDictionaryItem dictionaryItem)
    {
        var xml = new XElement(
            "DictionaryItem",
            new XAttribute("Key", dictionaryItem.Key),
            new XAttribute("Name", dictionaryItem.ItemKey));

        foreach (IDictionaryTranslation translation in dictionaryItem.Translations)
        {
            xml.Add(new XElement(
                "Value",
                new XAttribute("LanguageId", translation.Language!.Id),
                new XAttribute("LanguageCultureAlias", translation.Language.IsoCode),
                new XCData(translation.Value)));
        }

        return xml;
    }

    private IEnumerable<XElement> SerializePropertyTypes(
        IEnumerable<IPropertyType> propertyTypes,
        IEnumerable<PropertyGroup> propertyGroups)
    {
        foreach (IPropertyType propertyType in propertyTypes)
        {
            IDataType? definition = _dataTypeService.GetDataType(propertyType.DataTypeId);

            PropertyGroup? propertyGroup = propertyType.PropertyGroupId == null // true generic property
                ? null
                : propertyGroups.FirstOrDefault(x => x.Id == propertyType.PropertyGroupId.Value);

            XElement genericProperty = SerializePropertyType(propertyType, definition, propertyGroup);
            genericProperty.Add(new XElement("Variations", propertyType.Variations.ToString()));

            yield return genericProperty;
        }
    }

    private IEnumerable<XElement> SerializePropertyGroups(IEnumerable<PropertyGroup> propertyGroups)
    {
        foreach (PropertyGroup propertyGroup in propertyGroups)
        {
            yield return new XElement(
                "Tab", // TODO Rename to PropertyGroup
                new XElement("Id", propertyGroup.Id),
                new XElement("Key", propertyGroup.Key),
                new XElement("Type", propertyGroup.Type.ToString()),
                new XElement("Caption", propertyGroup.Name), // TODO Rename to Name (same in PackageDataInstallation)
                new XElement("Alias", propertyGroup.Alias),
                new XElement("SortOrder", propertyGroup.SortOrder));
        }
    }

    private XElement SerializePropertyType(IPropertyType propertyType, IDataType? definition, PropertyGroup? propertyGroup)
        => new(
            "GenericProperty",
            new XElement("Name", propertyType.Name),
            new XElement("Alias", propertyType.Alias),
            new XElement("Key", propertyType.Key),
            new XElement("Type", propertyType.PropertyEditorAlias),
            definition is not null ? new XElement("Definition", definition.Key) : null,
            propertyGroup is not null ? new XElement("Tab", propertyGroup.Name, new XAttribute("Alias", propertyGroup.Alias)) : null, // TODO Replace with PropertyGroupAlias
            new XElement("SortOrder", propertyType.SortOrder),
            new XElement("Mandatory", propertyType.Mandatory.ToString()),
            new XElement("LabelOnTop", propertyType.LabelOnTop.ToString()),
            propertyType.MandatoryMessage != null ? new XElement("MandatoryMessage", propertyType.MandatoryMessage) : null,
            propertyType.ValidationRegExp != null ? new XElement("Validation", propertyType.ValidationRegExp) : null,
            propertyType.ValidationRegExpMessage != null ? new XElement("ValidationRegExpMessage", propertyType.ValidationRegExpMessage) : null,
            propertyType.Description != null ? new XElement("Description", new XCData(propertyType.Description)) : null);

    private XElement SerializeCleanupPolicy(HistoryCleanup cleanupPolicy)
    {
        if (cleanupPolicy == null)
        {
            throw new ArgumentNullException(nameof(cleanupPolicy));
        }

        var element = new XElement(
            "HistoryCleanupPolicy",
            new XAttribute("preventCleanup", cleanupPolicy.PreventCleanup));

        if (cleanupPolicy.KeepAllVersionsNewerThanDays.HasValue)
        {
            element.Add(new XAttribute("keepAllVersionsNewerThanDays", cleanupPolicy.KeepAllVersionsNewerThanDays));
        }

        if (cleanupPolicy.KeepLatestVersionPerDayForDays.HasValue)
        {
            element.Add(new XAttribute("keepLatestVersionPerDayForDays", cleanupPolicy.KeepLatestVersionPerDayForDays));
        }

        return element;
    }

    // exports an IContentBase (IContent, IMedia or IMember) as an XElement.
    private XElement SerializeContentBase(IContentBase contentBase, string? urlValue, string nodeName, bool published)
    {
        var xml = new XElement(
            nodeName,
            new XAttribute("id", contentBase.Id.ToInvariantString()),
            new XAttribute("key", contentBase.Key),
            new XAttribute("parentID", (contentBase.Level > 1 ? contentBase.ParentId : -1).ToInvariantString()),
            new XAttribute("level", contentBase.Level),
            new XAttribute("creatorID", contentBase.CreatorId.ToInvariantString()),
            new XAttribute("sortOrder", contentBase.SortOrder),
            new XAttribute("createDate", contentBase.CreateDate.ToString("s")),
            new XAttribute("updateDate", contentBase.UpdateDate.ToString("s")),
            new XAttribute("nodeName", contentBase.Name!),
            new XAttribute("urlName", urlValue!),
            new XAttribute("path", contentBase.Path),
            new XAttribute("isDoc", string.Empty));

        // Add culture specific node names
        foreach (var culture in contentBase.AvailableCultures)
        {
            xml.Add(new XAttribute("nodeName-" + culture, contentBase.GetCultureName(culture)!));
        }

        foreach (IProperty property in contentBase.Properties)
        {
            xml.Add(SerializeProperty(property, published));
        }

        return xml;
    }

    // exports a property as XElements.
    private IEnumerable<XElement> SerializeProperty(IProperty property, bool published)
    {
        IPropertyType propertyType = property.PropertyType;

        // get the property editor for this property and let it convert it to the xml structure
        IDataEditor? propertyEditor = _propertyEditors[propertyType.PropertyEditorAlias];
        return propertyEditor == null
            ? Array.Empty<XElement>()
            : propertyEditor.GetValueEditor().ConvertDbToXml(property, published);
    }

    // exports an IContent item descendants.
    private void SerializeChildren(IEnumerable<IContent> children, XElement xml, bool published)
    {
        foreach (IContent child in children)
        {
            // add the child xml
            XElement childXml = Serialize(child, published);
            xml.Add(childXml);

            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                IEnumerable<IContent> grandChildren =
                    _contentService.GetPagedChildren(child.Id, page++, pageSize, out total);

                // recurse
                SerializeChildren(grandChildren, childXml, published);
            }
        }
    }

    // exports an IMedia item descendants.
    private void SerializeChildren(IEnumerable<IMedia> children, XElement xml, Action<IMedia, XElement>? onMediaItemSerialized)
    {
        foreach (IMedia child in children)
        {
            // add the child xml
            XElement childXml = Serialize(child, onMediaItemSerialized: onMediaItemSerialized);
            xml.Add(childXml);

            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                IEnumerable<IMedia> grandChildren =
                    _mediaService.GetPagedChildren(child.Id, page++, pageSize, out total);

                // recurse
                SerializeChildren(grandChildren, childXml, onMediaItemSerialized);
            }
        }
    }
}
