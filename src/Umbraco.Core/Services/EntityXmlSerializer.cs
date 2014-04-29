using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
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
        /// <param name="content">Content to export</param>
        /// <param name="deep">Optional parameter indicating whether to include descendents</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Content object</returns>
        public XElement Serialize(IContentService contentService, IDataTypeService dataTypeService, IContent content, bool deep = false)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : content.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var xml = Serialize(dataTypeService, content, nodeName);
            xml.Add(new XAttribute("nodeType", content.ContentType.Id));
            xml.Add(new XAttribute("creatorName", content.GetCreatorProfile().Name));
            xml.Add(new XAttribute("writerName", content.GetWriterProfile().Name));
            xml.Add(new XAttribute("writerID", content.WriterId));
            xml.Add(new XAttribute("template", content.Template == null ? "0" : content.Template.Id.ToString(CultureInfo.InvariantCulture)));
            xml.Add(new XAttribute("nodeTypeAlias", content.ContentType.Alias));

            if (deep)
            {
                var descendants = contentService.GetDescendants(content).ToArray();
                var currentChildren = descendants.Where(x => x.ParentId == content.Id);
                AddChildXml(contentService, dataTypeService, descendants, currentChildren, xml);
            }

            return xml;
        }

        /// <summary>
        /// Exports an <see cref="IMedia"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="mediaService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="media">Media to export</param>
        /// <param name="deep">Optional parameter indicating whether to include descendents</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Media object</returns>
        public XElement Serialize(IMediaService mediaService, IDataTypeService dataTypeService, IMedia media, bool deep = false)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : media.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var xml = Serialize(dataTypeService, media, nodeName);
            xml.Add(new XAttribute("nodeType", media.ContentType.Id));
            xml.Add(new XAttribute("writerName", media.GetCreatorProfile().Name));
            xml.Add(new XAttribute("writerID", media.CreatorId));
            xml.Add(new XAttribute("version", media.Version));
            xml.Add(new XAttribute("template", 0));
            xml.Add(new XAttribute("nodeTypeAlias", media.ContentType.Alias));

            if (deep)
            {
                var descendants = mediaService.GetDescendants(media).ToArray();
                var currentChildren = descendants.Where(x => x.ParentId == media.Id);
                AddChildXml(mediaService, dataTypeService, descendants, currentChildren, xml);
            }

            return xml;
        }

        /// <summary>
        /// Exports an <see cref="IMedia"/> item to xml as an <see cref="XElement"/>
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
            xml.Add(new XAttribute("key", member.Key));

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
        /// Used by Media Export to recursively add children
        /// </summary>
        /// <param name="mediaService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="originalDescendants"></param>
        /// <param name="currentChildren"></param>
        /// <param name="currentXml"></param>
        private void AddChildXml(IMediaService mediaService, IDataTypeService dataTypeService, IMedia[] originalDescendants, IEnumerable<IMedia> currentChildren, XElement currentXml)
        {
            foreach (var child in currentChildren)
            {
                //add the child's xml
                var childXml = Serialize(mediaService, dataTypeService, child);
                currentXml.Add(childXml);
                //copy local (out of closure)
                var c = child;
                //get this item's children                
                var children = originalDescendants.Where(x => x.ParentId == c.Id);
                //recurse and add it's children to the child xml element
                AddChildXml(mediaService, dataTypeService, originalDescendants, children, childXml);
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
        /// <param name="originalDescendants"></param>
        /// <param name="currentChildren"></param>
        /// <param name="currentXml"></param>
        private void AddChildXml(IContentService contentService, IDataTypeService dataTypeService, IContent[] originalDescendants, IEnumerable<IContent> currentChildren, XElement currentXml)
        {
            foreach (var child in currentChildren)
            {
                //add the child's xml
                var childXml = Serialize(contentService, dataTypeService, child);
                currentXml.Add(childXml);
                //copy local (out of closure)
                var c = child;
                //get this item's children                
                var children = originalDescendants.Where(x => x.ParentId == c.Id);
                //recurse and add it's children to the child xml element
                AddChildXml(contentService, dataTypeService, originalDescendants, children, childXml);
            }
        }
    }
}