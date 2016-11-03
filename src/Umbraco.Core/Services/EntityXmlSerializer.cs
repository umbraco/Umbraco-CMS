using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using umbraco.interfaces;

namespace Umbraco.Core.Services
{
    //TODO: Move the rest of the logic for the PackageService.Export methods to here!

    /// <summary>
    /// A helper class to serialize entities to XML
    /// </summary>
    internal class EntityXmlSerializer
    {
        /// <summary>
        /// Exports an <see cref="IContent"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="userService"></param>
        /// <param name="content">Content to export</param>
        /// <param name="deep">Optional parameter indicating whether to include descendents</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Content object</returns>
        public XElement Serialize(IContentService contentService, IDataTypeService dataTypeService, IUserService userService, IContent content, bool deep = false)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : content.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var xml = Serialize(dataTypeService, content, nodeName);
            xml.Add(new XAttribute("nodeType", content.ContentType.Id));
            xml.Add(new XAttribute("creatorName", content.GetCreatorProfile(userService).Name));
            xml.Add(new XAttribute("writerName", content.GetWriterProfile(userService).Name));
            xml.Add(new XAttribute("writerID", content.WriterId));
            xml.Add(new XAttribute("template", content.Template == null ? "0" : content.Template.Id.ToString(CultureInfo.InvariantCulture)));
            xml.Add(new XAttribute("nodeTypeAlias", content.ContentType.Alias));
            xml.Add(new XAttribute("isPublished", content.Published));

            if (deep)
            {
                var descendants = contentService.GetDescendants(content).ToArray();
                var currentChildren = descendants.Where(x => x.ParentId == content.Id);
                AddChildXml(contentService, dataTypeService, userService, descendants, currentChildren, xml);
            }

            return xml;
        }

        /// <summary>
        /// Exports an <see cref="IMedia"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="mediaService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="userService"></param>
        /// <param name="media">Media to export</param>
        /// <param name="deep">Optional parameter indicating whether to include descendents</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Media object</returns>
        public XElement Serialize(IMediaService mediaService, IDataTypeService dataTypeService, IUserService userService, IMedia media, bool deep = false)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : media.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var xml = Serialize(dataTypeService, media, nodeName);
            xml.Add(new XAttribute("nodeType", media.ContentType.Id));
            xml.Add(new XAttribute("writerName", media.GetCreatorProfile(userService).Name));
            xml.Add(new XAttribute("writerID", media.CreatorId));
            xml.Add(new XAttribute("version", media.Version));
            xml.Add(new XAttribute("template", 0));
            xml.Add(new XAttribute("nodeTypeAlias", media.ContentType.Alias));

            if (deep)
            {
                var descendants = mediaService.GetDescendants(media).ToArray();
                var currentChildren = descendants.Where(x => x.ParentId == media.Id);
                AddChildXml(mediaService, dataTypeService, userService, descendants, currentChildren, xml);
            }

            return xml;
        }

        /// <summary>
        /// Exports an <see cref="IMember"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="dataTypeService"></param>
        /// <param name="member">Member to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Member object</returns>
        public XElement Serialize(IDataTypeService dataTypeService, IMember member)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : member.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var xml = Serialize(dataTypeService, member, nodeName);
            xml.Add(new XAttribute("nodeType", member.ContentType.Id));
            xml.Add(new XAttribute("nodeTypeAlias", member.ContentType.Alias));

            xml.Add(new XAttribute("loginName", member.Username));
            xml.Add(new XAttribute("email", member.Email));
            
            xml.Add(new XAttribute("icon", member.ContentType.Icon));

            return xml;
        }

        public XElement Serialize(IDataTypeService dataTypeService, Property property)
        {
            var propertyType = property.PropertyType;
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "data" : property.Alias.ToSafeAlias();

            var xElement = new XElement(nodeName);

            //Add the property alias to the legacy schema
            if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
            {
                var a = new XAttribute("alias", property.Alias.ToSafeAlias());
                xElement.Add(a);
            }

            //Get the property editor for thsi property and let it convert it to the xml structure
            var propertyEditor = PropertyEditorResolver.Current.GetByAlias(property.PropertyType.PropertyEditorAlias);
            if (propertyEditor != null)
            {
                var xmlValue = propertyEditor.ValueEditor.ConvertDbToXml(property, propertyType, dataTypeService);
                xElement.Add(xmlValue);
            }

            return xElement;
        }

        /// <summary>
        /// Exports an <see cref="IDataTypeDefinition"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="dataTypeService"></param>
        /// <param name="dataTypeDefinition">IDataTypeDefinition type to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDataTypeDefinition object</returns>
        public XElement Serialize(IDataTypeService dataTypeService, IDataTypeDefinition dataTypeDefinition)
        {
            var prevalues = new XElement("PreValues");
            var prevalueList = dataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinition.Id)
                .FormatAsDictionary();

            var sort = 0;
            foreach (var pv in prevalueList)
            {
                var prevalue = new XElement("PreValue");
                prevalue.Add(new XAttribute("Id", pv.Value.Id));
                prevalue.Add(new XAttribute("Value", pv.Value.Value ?? ""));
                prevalue.Add(new XAttribute("Alias", pv.Key));
                prevalue.Add(new XAttribute("SortOrder", sort));
                prevalues.Add(prevalue);
                sort++;
            }

            var xml = new XElement("DataType", prevalues);
            xml.Add(new XAttribute("Name", dataTypeDefinition.Name));
            //The 'ID' when exporting is actually the property editor alias (in pre v7 it was the IDataType GUID id)
            xml.Add(new XAttribute("Id", dataTypeDefinition.PropertyEditorAlias));
            xml.Add(new XAttribute("Definition", dataTypeDefinition.Key));
            xml.Add(new XAttribute("DatabaseType", dataTypeDefinition.DatabaseType.ToString()));

            var folderNames = string.Empty;
            if (dataTypeDefinition.Level != 1)
            {
                //get url encoded folder names
                var folders = dataTypeService.GetContainers(dataTypeDefinition)
                    .OrderBy(x => x.Level)
                    .Select(x => HttpUtility.UrlEncode(x.Name));

                folderNames = string.Join("/", folders.ToArray());
            }

            if (string.IsNullOrWhiteSpace(folderNames) == false)
                xml.Add(new XAttribute("Folders", folderNames));            

            return xml;
        }

        public XElement Serialize(IDictionaryItem dictionaryItem)
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

            var concreteTemplate = template as Template;
            if (concreteTemplate != null && concreteTemplate.MasterTemplateId != null)
            {
                if (concreteTemplate.MasterTemplateId.IsValueCreated &&
                    concreteTemplate.MasterTemplateId.Value != default(int))
                {
                    xml.Add(new XElement("Master", concreteTemplate.MasterTemplateId.ToString()));
                    xml.Add(new XElement("MasterAlias", concreteTemplate.MasterTemplateAlias));
                }
            }

            return xml;
        }

        public XElement Serialize(IDataTypeService dataTypeService, IMediaType mediaType)
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

            var genericProperties = new XElement("GenericProperties"); // actually, all of them
            foreach (var propertyType in mediaType.PropertyTypes)
            {
                var definition = dataTypeService.GetDataTypeDefinitionById(propertyType.DataTypeDefinitionId);

                var propertyGroup = propertyType.PropertyGroupId == null // true generic property
                    ? null
                    : mediaType.PropertyGroups.FirstOrDefault(x => x.Id == propertyType.PropertyGroupId.Value);

                var genericProperty = new XElement("GenericProperty",
                                                   new XElement("Name", propertyType.Name),
                                                   new XElement("Alias", propertyType.Alias),
                                                   new XElement("Type", propertyType.PropertyEditorAlias),
                                                   new XElement("Definition", definition.Key),
                                                   new XElement("Tab", propertyGroup == null ? "" : propertyGroup.Name),
                                                   new XElement("Mandatory", propertyType.Mandatory.ToString()),
                                                   new XElement("Validation", propertyType.ValidationRegExp),
                                                   new XElement("Description", new XCData(propertyType.Description)));
                genericProperties.Add(genericProperty);
            }

            var tabs = new XElement("Tabs");
            foreach (var propertyGroup in mediaType.PropertyGroups)
            {
                var tab = new XElement("Tab",
                                       new XElement("Id", propertyGroup.Id.ToString(CultureInfo.InvariantCulture)),
                                       new XElement("Caption", propertyGroup.Name),
                                       new XElement("SortOrder", propertyGroup.SortOrder));

                tabs.Add(tab);
            }

            var xml = new XElement("MediaType",
                                   info,
                                   structure,
                                   genericProperties,
                                   tabs);

            return xml;
        }

        public XElement Serialize(IMacro macro)
        {
            var xml = new XElement("macro");
            xml.Add(new XElement("name", macro.Name));
            xml.Add(new XElement("alias", macro.Alias));
            xml.Add(new XElement("scriptType", macro.ControlType));
            xml.Add(new XElement("scriptAssembly", macro.ControlAssembly));
            xml.Add(new XElement("scriptingFile", macro.ScriptPath));
            xml.Add(new XElement("xslt", macro.XsltPath));
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

        /// <summary>
        /// Exports an <see cref="IContentType"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="dataTypeService"></param>
        /// <param name="contentTypeService"></param>
        /// <param name="contentType">Content type to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IContentType object</returns>
        public XElement Serialize(IDataTypeService dataTypeService, IContentTypeService contentTypeService, IContentType contentType)
        {
            var info = new XElement("Info",
                                    new XElement("Name", contentType.Name),
                                    new XElement("Alias", contentType.Alias),
                                    new XElement("Icon", contentType.Icon),
                                    new XElement("Thumbnail", contentType.Thumbnail),
                                    new XElement("Description", contentType.Description),
                                    new XElement("AllowAtRoot", contentType.AllowedAsRoot.ToString()),
                                    new XElement("IsListView", contentType.IsContainer.ToString()));

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

            var genericProperties = new XElement("GenericProperties"); // actually, all of them
            foreach (var propertyType in contentType.PropertyTypes)
            {
                var definition = dataTypeService.GetDataTypeDefinitionById(propertyType.DataTypeDefinitionId);

                var propertyGroup = propertyType.PropertyGroupId == null // true generic property
                    ? null
                    : contentType.PropertyGroups.FirstOrDefault(x => x.Id == propertyType.PropertyGroupId.Value);

                var genericProperty = new XElement("GenericProperty",
                                                   new XElement("Name", propertyType.Name),
                                                   new XElement("Alias", propertyType.Alias),
                                                   new XElement("Type", propertyType.PropertyEditorAlias),
                                                   new XElement("Definition", definition.Key),
                                                   new XElement("Tab", propertyGroup == null ? "" : propertyGroup.Name),
                                                   new XElement("SortOrder", propertyType.SortOrder),
                                                   new XElement("Mandatory", propertyType.Mandatory.ToString()),
                                                   propertyType.ValidationRegExp != null ? new XElement("Validation", propertyType.ValidationRegExp) : null,
                                                   propertyType.Description != null ? new XElement("Description", new XCData(propertyType.Description)) : null);
                
                genericProperties.Add(genericProperty);
            }

            var tabs = new XElement("Tabs");
            foreach (var propertyGroup in contentType.PropertyGroups)
            {
                var tab = new XElement("Tab",
                                       new XElement("Id", propertyGroup.Id.ToString(CultureInfo.InvariantCulture)),
                                       new XElement("Caption", propertyGroup.Name),
                                       new XElement("SortOrder", propertyGroup.SortOrder));
                tabs.Add(tab);
            }

            var xml = new XElement("DocumentType",
                info,
                structure,
                genericProperties,
                tabs);

            var folderNames = string.Empty;
            //don't add folders if this is a child doc type
            if (contentType.Level != 1 && masterContentType == null)
            {
                //get url encoded folder names
                var folders = contentTypeService.GetContentTypeContainers(contentType)
                    .OrderBy(x => x.Level)
                    .Select(x => HttpUtility.UrlEncode(x.Name));

                folderNames = string.Join("/", folders.ToArray());
            }

            if (string.IsNullOrWhiteSpace(folderNames) == false)
                xml.Add(new XAttribute("Folders", folderNames));

            return xml;
        }

        /// <summary>
        /// Used by Media Export to recursively add children
        /// </summary>
        /// <param name="mediaService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="userService"></param>
        /// <param name="originalDescendants"></param>
        /// <param name="currentChildren"></param>
        /// <param name="currentXml"></param>
        private void AddChildXml(IMediaService mediaService, IDataTypeService dataTypeService, IUserService userService, IMedia[] originalDescendants, IEnumerable<IMedia> currentChildren, XElement currentXml)
        {
            foreach (var child in currentChildren)
            {
                //add the child's xml
                var childXml = Serialize(mediaService, dataTypeService, userService, child);
                currentXml.Add(childXml);
                //copy local (out of closure)
                var c = child;
                //get this item's children                
                var children = originalDescendants.Where(x => x.ParentId == c.Id);
                //recurse and add it's children to the child xml element
                AddChildXml(mediaService, dataTypeService, userService, originalDescendants, children, childXml);
            }
        }

        /// <summary>
        /// Part of the export of IContent and IMedia and IMember which is shared
        /// </summary>
        /// <param name="dataTypeService"></param>
        /// <param name="contentBase">Base Content or Media to export</param>
        /// <param name="nodeName">Name of the node</param>
        /// <returns><see cref="XElement"/></returns>
        private XElement Serialize(IDataTypeService dataTypeService, IContentBase contentBase, string nodeName)
        {
            //NOTE: that one will take care of umbracoUrlName
            var url = contentBase.GetUrlSegment();

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
                new XAttribute("urlName", url),
                new XAttribute("path", contentBase.Path),
                new XAttribute("isDoc", ""));

            foreach (var property in contentBase.Properties.Where(p => p != null && p.Value != null && p.Value.ToString().IsNullOrWhiteSpace() == false))
            {
                xml.Add(Serialize(dataTypeService, property));
            }

            return xml;
        }

        /// <summary>
        /// Used by Content Export to recursively add children
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="userService"></param>
        /// <param name="originalDescendants"></param>
        /// <param name="currentChildren"></param>
        /// <param name="currentXml"></param>
        private void AddChildXml(IContentService contentService, IDataTypeService dataTypeService, IUserService userService, IContent[] originalDescendants, IEnumerable<IContent> currentChildren, XElement currentXml)
        {
            foreach (var child in currentChildren)
            {
                //add the child's xml
                var childXml = Serialize(contentService, dataTypeService, userService, child);
                currentXml.Add(childXml);
                //copy local (out of closure)
                var c = child;
                //get this item's children                
                var children = originalDescendants.Where(x => x.ParentId == c.Id);
                //recurse and add it's children to the child xml element
                AddChildXml(contentService, dataTypeService, userService, originalDescendants, children, childXml);
            }
        }
    }
}