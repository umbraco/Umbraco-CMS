using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Serializes entities to XML
    /// </summary>
    internal class EntityXmlSerializer : IEntityXmlSerializer
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IMediaService _mediaService;
        private readonly IContentService _contentService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IUserService _userService;
        private readonly ILocalizationService _localizationService;
        private readonly UrlSegmentProviderCollection _urlSegmentProviders;

        public EntityXmlSerializer(
            IContentService contentService,
            IMediaService mediaService,
            IDataTypeService dataTypeService,
            IUserService userService,
            ILocalizationService localizationService,
            IContentTypeService contentTypeService,
            UrlSegmentProviderCollection urlSegmentProviders)
        {
            _contentTypeService = contentTypeService;
            _mediaService = mediaService;
            _contentService = contentService;
            _dataTypeService = dataTypeService;
            _userService = userService;
            _localizationService = localizationService;
            _urlSegmentProviders = urlSegmentProviders;
        }

        /// <summary>
        /// Exports an IContent item as an XElement.
        /// </summary>
        public XElement Serialize(IContent content,
            bool published,
            bool withDescendants = false) // TODO: take care of usage! only used for the packager
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var nodeName = content.ContentType.Alias.ToSafeAlias();

            var xml = SerializeContentBase(content, content.GetUrlSegment(_urlSegmentProviders), nodeName, published);

            xml.Add(new XAttribute("nodeType", content.ContentType.Id));
            xml.Add(new XAttribute("nodeTypeAlias", content.ContentType.Alias));

            xml.Add(new XAttribute("creatorName", content.GetCreatorProfile(_userService)?.Name ?? "??"));
            //xml.Add(new XAttribute("creatorID", content.CreatorId));
            xml.Add(new XAttribute("writerName", content.GetWriterProfile(_userService)?.Name ?? "??"));
            xml.Add(new XAttribute("writerID", content.WriterId));

            xml.Add(new XAttribute("template", content.TemplateId?.ToString(CultureInfo.InvariantCulture) ?? ""));

            xml.Add(new XAttribute("isPublished", content.Published));

            if (withDescendants)
            {
                const int pageSize = 500;
                var page = 0;
                var total = long.MaxValue;
                while(page * pageSize < total)
                {
                    var children = _contentService.GetPagedChildren(content.Id, page++, pageSize, out total);
                    SerializeChildren(children, xml, published);
                }

            }

            return xml;
        }

        /// <summary>
        /// Exports an IMedia item as an XElement.
        /// </summary>
        public XElement Serialize(
            IMedia media,
            bool withDescendants = false)
        {
            if (_mediaService == null) throw new ArgumentNullException(nameof(_mediaService));
            if (_dataTypeService == null) throw new ArgumentNullException(nameof(_dataTypeService));
            if (_userService == null) throw new ArgumentNullException(nameof(_userService));
            if (_localizationService == null) throw new ArgumentNullException(nameof(_localizationService));
            if (media == null) throw new ArgumentNullException(nameof(media));
            if (_urlSegmentProviders == null) throw new ArgumentNullException(nameof(_urlSegmentProviders));

            var nodeName = media.ContentType.Alias.ToSafeAlias();

            const bool published = false; // always false for media
            var xml = SerializeContentBase(media, media.GetUrlSegment(_urlSegmentProviders), nodeName, published);

            xml.Add(new XAttribute("nodeType", media.ContentType.Id));
            xml.Add(new XAttribute("nodeTypeAlias", media.ContentType.Alias));

            //xml.Add(new XAttribute("creatorName", media.GetCreatorProfile(userService).Name));
            //xml.Add(new XAttribute("creatorID", media.CreatorId));
            xml.Add(new XAttribute("writerName", media.GetWriterProfile(_userService)?.Name ?? string.Empty));
            xml.Add(new XAttribute("writerID", media.WriterId));

            //xml.Add(new XAttribute("template", 0)); // no template for media

            if (withDescendants)
            {
                const int pageSize = 500;
                var page = 0;
                var total = long.MaxValue;
                while (page * pageSize < total)
                {
                    var children = _mediaService.GetPagedChildren(media.Id, page++, pageSize, out total);
                    SerializeChildren(children, xml);
                }
            }

            return xml;
        }

        /// <summary>
        /// Exports an IMember item as an XElement.
        /// </summary>
        public XElement Serialize(IMember member)
        {
            var nodeName = member.ContentType.Alias.ToSafeAlias();

            const bool published = false; // always false for member
            var xml = SerializeContentBase(member, "", nodeName, published);

            xml.Add(new XAttribute("nodeType", member.ContentType.Id));
            xml.Add(new XAttribute("nodeTypeAlias", member.ContentType.Alias));

            // what about writer/creator/version?

            xml.Add(new XAttribute("loginName", member.Username));
            xml.Add(new XAttribute("email", member.Email));
            xml.Add(new XAttribute("icon", member.ContentType.Icon));

            return xml;
        }

        /// <summary>
        /// Exports a list of Data Types
        /// </summary>
        /// <param name="dataTypeDefinitions">List of data types to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDataTypeDefinition objects</returns>
        public XElement Serialize(IEnumerable<IDataType> dataTypeDefinitions)
        {
            var container = new XElement("DataTypes");
            foreach (var dataTypeDefinition in dataTypeDefinitions)
            {
                container.Add(Serialize(dataTypeDefinition));
            }
            return container;
        }

        public XElement Serialize(IDataType dataType)
        {
            var xml = new XElement("DataType");
            xml.Add(new XAttribute("Name", dataType.Name));
            //The 'ID' when exporting is actually the property editor alias (in pre v7 it was the IDataType GUID id)
            xml.Add(new XAttribute("Id", dataType.EditorAlias));
            xml.Add(new XAttribute("Definition", dataType.Key));
            xml.Add(new XAttribute("DatabaseType", dataType.DatabaseType.ToString()));
            xml.Add(new XAttribute("Configuration", JsonConvert.SerializeObject(dataType.Configuration, PropertyEditors.ConfigurationEditor.ConfigurationJsonSettings)));

            var folderNames = string.Empty;
            if (dataType.Level != 1)
            {
                //get URL encoded folder names
                var folders = _dataTypeService.GetContainers(dataType)
                    .OrderBy(x => x.Level)
                    .Select(x => HttpUtility.UrlEncode(x.Name));

                folderNames = string.Join("/", folders.ToArray());
            }

            if (string.IsNullOrWhiteSpace(folderNames) == false)
                xml.Add(new XAttribute("Folders", folderNames));

            return xml;
        }

        /// <summary>
        /// Exports a list of <see cref="IDictionaryItem"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="dictionaryItem">List of dictionary items to export</param>
        /// <param name="includeChildren">Optional boolean indicating whether or not to include children</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDictionaryItem objects</returns>
        public XElement Serialize(IEnumerable<IDictionaryItem> dictionaryItem, bool includeChildren = true)
        {
            var xml = new XElement("DictionaryItems");
            foreach (var item in dictionaryItem)
            {
                xml.Add(Serialize(item, includeChildren));
            }
            return xml;
        }

        /// <summary>
        /// Exports a single <see cref="IDictionaryItem"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="dictionaryItem">Dictionary Item to export</param>
        /// <param name="includeChildren">Optional boolean indicating whether or not to include children</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDictionaryItem object</returns>
        public XElement Serialize(IDictionaryItem dictionaryItem, bool includeChildren)
        {
            var xml = Serialize(dictionaryItem);

            if (includeChildren)
            {
                var children = _localizationService.GetDictionaryItemChildren(dictionaryItem.Key);
                foreach (var child in children)
                {
                    xml.Add(Serialize(child, true));
                }
            }

            return xml;
        }

        private XElement Serialize(IDictionaryItem dictionaryItem)
        {
            var xml = new XElement("DictionaryItem", new XAttribute("Key", dictionaryItem.ItemKey));
            foreach (var translation in dictionaryItem.Translations)
            {
                xml.Add(new XElement("Value",
                    new XAttribute("LanguageId", translation.Language.Id),
                    new XAttribute("LanguageCultureAlias", translation.Language.IsoCode),
                    new XCData(translation.Value)));
            }

            return xml;
        }

        public XElement Serialize(Stylesheet stylesheet)
        {
            var xml = new XElement("Stylesheet",
                new XElement("Name", stylesheet.Alias),
                new XElement("FileName", stylesheet.Path),
                new XElement("Content", new XCData(stylesheet.Content)));

            var props = new XElement("Properties");
            xml.Add(props);

            foreach (var prop in stylesheet.Properties)
            {
                props.Add(new XElement("Property",
                    new XElement("Name", prop.Name),
                    new XElement("Alias", prop.Alias),
                    new XElement("Value", prop.Value)));
            }

            return xml;
        }

        /// <summary>
        /// Exports a list of <see cref="ILanguage"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="languages">List of Languages to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ILanguage objects</returns>
        public XElement Serialize(IEnumerable<ILanguage> languages)
        {
            var xml = new XElement("Languages");
            foreach (var language in languages)
            {
                xml.Add(Serialize(language));
            }
            return xml;
        }

        public XElement Serialize(ILanguage language)
        {
            var xml = new XElement("Language",
                new XAttribute("Id", language.Id),
                new XAttribute("CultureAlias", language.IsoCode),
                new XAttribute("FriendlyName", language.CultureName));

            return xml;
        }

        public XElement Serialize(ITemplate template)
        {
            var xml = new XElement("Template");
            xml.Add(new XElement("Name", template.Name));
            xml.Add(new XElement("Alias", template.Alias));
            xml.Add(new XElement("Design", new XCData(template.Content)));

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
        /// Exports a list of <see cref="ITemplate"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="templates"></param>
        /// <returns></returns>
        public XElement Serialize(IEnumerable<ITemplate> templates)
        {
            var xml = new XElement("Templates");
            foreach (var item in templates)
            {
                xml.Add(Serialize(item));
            }
            return xml;
        }

        public XElement Serialize(IMediaType mediaType)
        {
            var info = new XElement("Info",
                                    new XElement("Name", mediaType.Name),
                                    new XElement("Alias", mediaType.Alias),
                                    new XElement("Icon", mediaType.Icon),
                                    new XElement("Thumbnail", mediaType.Thumbnail),
                                    new XElement("Description", mediaType.Description),
                                    new XElement("AllowAtRoot", mediaType.AllowedAsRoot.ToString()));

            var masterContentType = mediaType.CompositionAliases().FirstOrDefault();
            if (masterContentType != null)
                info.Add(new XElement("Master", masterContentType));

            var structure = new XElement("Structure");
            foreach (var allowedType in mediaType.AllowedContentTypes)
            {
                structure.Add(new XElement("MediaType", allowedType.Alias));
            }

            var genericProperties = new XElement("GenericProperties", SerializePropertyTypes(mediaType.PropertyTypes, mediaType.PropertyGroups)); // actually, all of them

            var tabs = new XElement("Tabs", SerializePropertyGroups(mediaType.PropertyGroups)); // TODO Rename to PropertyGroups

            var xml = new XElement("MediaType",
                                   info,
                                   structure,
                                   genericProperties,
                                   tabs);

            return xml;
        }

        /// <summary>
        /// Exports a list of <see cref="IMacro"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="macros">Macros to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IMacro objects</returns>
        public XElement Serialize(IEnumerable<IMacro> macros)
        {
            var xml = new XElement("Macros");
            foreach (var item in macros)
            {
                xml.Add(Serialize(item));
            }
            return xml;
        }

        public XElement Serialize(IMacro macro)
        {
            var xml = new XElement("macro");
            xml.Add(new XElement("name", macro.Name));
            xml.Add(new XElement("alias", macro.Alias));
            xml.Add(new XElement("macroType", macro.MacroType));
            xml.Add(new XElement("macroSource", macro.MacroSource));
            xml.Add(new XElement("useInEditor", macro.UseInEditor.ToString()));
            xml.Add(new XElement("dontRender", macro.DontRender.ToString()));
            xml.Add(new XElement("refreshRate", macro.CacheDuration.ToString(CultureInfo.InvariantCulture)));
            xml.Add(new XElement("cacheByMember", macro.CacheByMember.ToString()));
            xml.Add(new XElement("cacheByPage", macro.CacheByPage.ToString()));

            var properties = new XElement("properties");
            foreach (var property in macro.Properties)
            {
                properties.Add(new XElement("property",
                    new XAttribute("name", property.Name),
                    new XAttribute("alias", property.Alias),
                    new XAttribute("sortOrder", property.SortOrder),
                    new XAttribute("propertyType", property.EditorAlias)));
            }
            xml.Add(properties);

            return xml;
        }

        public XElement Serialize(IContentType contentType)
        {
            var info = new XElement("Info",
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

            var masterContentType = contentType.ContentTypeComposition.FirstOrDefault(x => x.Id == contentType.ParentId);
            if(masterContentType != null)
                info.Add(new XElement("Master", masterContentType.Alias));

            var compositionsElement = new XElement("Compositions");
            var compositions = contentType.ContentTypeComposition;
            foreach (var composition in compositions)
            {
                compositionsElement.Add(new XElement("Composition", composition.Alias));
            }
            info.Add(compositionsElement);

            var allowedTemplates = new XElement("AllowedTemplates");
            foreach (var template in contentType.AllowedTemplates)
            {
                allowedTemplates.Add(new XElement("Template", template.Alias));
            }
            info.Add(allowedTemplates);

            if (contentType.DefaultTemplate != null && contentType.DefaultTemplate.Id != 0)
                info.Add(new XElement("DefaultTemplate", contentType.DefaultTemplate.Alias));
            else
                info.Add(new XElement("DefaultTemplate", ""));

            var structure = new XElement("Structure");
            foreach (var allowedType in contentType.AllowedContentTypes)
            {
                structure.Add(new XElement("DocumentType", allowedType.Alias));
            }

            var genericProperties = new XElement("GenericProperties", SerializePropertyTypes(contentType.PropertyTypes, contentType.PropertyGroups)); // actually, all of them

            var tabs = new XElement("Tabs", SerializePropertyGroups(contentType.PropertyGroups)); // TODO Rename to PropertyGroups

       

            var xml = new XElement("DocumentType",
                info,
                structure,
                genericProperties,
                tabs);

            if (contentType.HistoryCleanup != null)
            {
                xml.Add(SerializeCleanupPolicy(contentType.HistoryCleanup));
            }

            var folderNames = string.Empty;
            //don't add folders if this is a child doc type
            if (contentType.Level != 1 && masterContentType == null)
            {
                //get URL encoded folder names
                var folders = _contentTypeService.GetContainers(contentType)
                    .OrderBy(x => x.Level)
                    .Select(x => HttpUtility.UrlEncode(x.Name));

                folderNames = string.Join("/", folders.ToArray());
            }

            if (string.IsNullOrWhiteSpace(folderNames) == false)
                xml.Add(new XAttribute("Folders", folderNames));

            return xml;
        }

        private IEnumerable<XElement> SerializePropertyTypes(IEnumerable<PropertyType> propertyTypes, IEnumerable<PropertyGroup> propertyGroups)
        {
            foreach (var propertyType in propertyTypes)
            {
                var definition = _dataTypeService.GetDataType(propertyType.DataTypeId);

                var propertyGroup = propertyType.PropertyGroupId == null // true generic property
                    ? null
                    : propertyGroups.FirstOrDefault(x => x.Id == propertyType.PropertyGroupId.Value);

                var genericProperty = new XElement("GenericProperty",
                    new XElement("Name", propertyType.Name),
                    new XElement("Alias", propertyType.Alias),
                    new XElement("Key", propertyType.Key),
                    new XElement("Type", propertyType.PropertyEditorAlias),
                    new XElement("Definition", definition.Key),
                    propertyGroup != null ? new XElement("Tab", propertyGroup.Name, new XAttribute("Alias", propertyGroup.Alias)) : null, // TODO Replace with PropertyGroupAlias
                    new XElement("SortOrder", propertyType.SortOrder),
                    new XElement("Mandatory", propertyType.Mandatory.ToString()),
                    new XElement("LabelOnTop", propertyType.LabelOnTop.ToString()),
                    propertyType.MandatoryMessage != null ? new XElement("MandatoryMessage", propertyType.MandatoryMessage) : null,
                    propertyType.ValidationRegExp != null ? new XElement("Validation", propertyType.ValidationRegExp) : null,
                    propertyType.ValidationRegExpMessage != null ? new XElement("ValidationRegExpMessage", propertyType.ValidationRegExpMessage) : null,
                    propertyType.Description != null ? new XElement("Description", new XCData(propertyType.Description)) : null,
                    new XElement("Variations", propertyType.Variations.ToString()));

                yield return genericProperty;
            }
        }

        private IEnumerable<XElement> SerializePropertyGroups(IEnumerable<PropertyGroup> propertyGroups)
        {
            foreach (var propertyGroup in propertyGroups)
            {
                yield return new XElement("Tab", // TODO Rename to PropertyGroup
                    new XElement("Id", propertyGroup.Id),
                    new XElement("Key", propertyGroup.Key),
                    new XElement("Type", propertyGroup.Type.ToString()),
                    new XElement("Caption", propertyGroup.Name), // TODO Rename to Name (same in PackageDataInstallation)
                    new XElement("Alias", propertyGroup.Alias),
                    new XElement("SortOrder", propertyGroup.SortOrder));
            }
        }

        private XElement SerializeCleanupPolicy(HistoryCleanup cleanupPolicy)
        {
            if (cleanupPolicy == null)
            {
                throw new ArgumentNullException(nameof(cleanupPolicy));
            }

            var element = new XElement("HistoryCleanupPolicy",
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
        private XElement SerializeContentBase(IContentBase contentBase, string urlValue, string nodeName, bool published)
        {
            var xml = new XElement(nodeName,
                new XAttribute("id", contentBase.Id),
                new XAttribute("key", contentBase.Key),
                new XAttribute("parentID", contentBase.Level > 1 ? contentBase.ParentId : -1),
                new XAttribute("level", contentBase.Level),
                new XAttribute("creatorID", contentBase.CreatorId),
                new XAttribute("sortOrder", contentBase.SortOrder),
                new XAttribute("createDate", contentBase.CreateDate.ToString("s")),
                new XAttribute("updateDate", contentBase.UpdateDate.ToString("s")),
                new XAttribute("nodeName", contentBase.Name),
                new XAttribute("urlName", urlValue),
                new XAttribute("path", contentBase.Path),
                new XAttribute("isDoc", ""));


            // Add culture specific node names
            foreach (var culture in contentBase.AvailableCultures)
            {
                xml.Add(new XAttribute("nodeName-" + culture, contentBase.GetCultureName(culture)));
            }

            foreach (var property in contentBase.Properties)
                xml.Add(SerializeProperty(property, published));

            return xml;
        }

        // exports a property as XElements.
        private IEnumerable<XElement> SerializeProperty(Property property, bool published)
        {
            var propertyType = property.PropertyType;

            // get the property editor for this property and let it convert it to the xml structure
            var propertyEditor = Current.PropertyEditors[propertyType.PropertyEditorAlias];
            return propertyEditor == null
                ? Array.Empty<XElement>()
                : propertyEditor.GetValueEditor().ConvertDbToXml(property, _dataTypeService, _localizationService, published);
        }

        // exports an IContent item descendants.
        private void SerializeChildren(IEnumerable<IContent> children, XElement xml, bool published)
        {
            foreach (var child in children)
            {
                // add the child xml
                var childXml = Serialize(child, published);
                xml.Add(childXml);

                const int pageSize = 500;
                var page = 0;
                var total = long.MaxValue;
                while(page * pageSize < total)
                {
                    var grandChildren = _contentService.GetPagedChildren(child.Id, page++, pageSize, out total);
                    // recurse
                    SerializeChildren(grandChildren, childXml, published);
                }
            }
        }

        // exports an IMedia item descendants.
        private void SerializeChildren(IEnumerable<IMedia> children, XElement xml)
        {
            foreach (var child in children)
            {
                // add the child xml
                var childXml = Serialize(child);
                xml.Add(childXml);

                const int pageSize = 500;
                var page = 0;
                var total = long.MaxValue;
                while (page * pageSize < total)
                {
                    var grandChildren = _mediaService.GetPagedChildren(child.Id, page++, pageSize, out total);
                    // recurse
                    SerializeChildren(grandChildren, childXml);
                }
            }
        }
    }
}
