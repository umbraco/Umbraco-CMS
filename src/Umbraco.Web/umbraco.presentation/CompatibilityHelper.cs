using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Web.umbraco.presentation
{
    static class CompatibilityHelper
    {
        // NOTE - moved from umbraco.MacroEngines to avoid circ. references

        public static INode ConvertToNode(IPublishedContent doc)
        {
            var node = new ConvertedNode(doc);
            return node;
        }

        private static IProperty ConvertToNodeProperty(IPublishedProperty prop)
        {
            return new ConvertedProperty(prop);
        }

        private class ConvertedNode : INode
        {
            private readonly IPublishedContent _doc;

            public ConvertedNode(IPublishedContent doc)
            {
                _doc = doc;

                if (doc == null)
                {
                    Id = 0;
                    return;
                }

                template = doc.TemplateId;
                Id = doc.Id;
                Path = doc.Path;
                CreatorName = doc.CreatorName;
                SortOrder = doc.SortOrder;
                UpdateDate = doc.UpdateDate;
                Name = doc.Name;
                NodeTypeAlias = doc.DocumentTypeAlias;
                CreateDate = doc.CreateDate;
                CreatorID = doc.CreatorId;
                Level = doc.Level;
                UrlName = doc.UrlName;
                Version = doc.Version;
                WriterID = doc.WriterId;
                WriterName = doc.WriterName;
            }

            public INode Parent
            {
                get { return _doc.Parent == null ? null : ConvertToNode(_doc.Parent); }
            }
            public int Id { get; private set; }
            public int template { get; private set; }
            public int SortOrder { get; private set; }
            public string Name { get; private set; }
            public string UrlName { get; private set; }
            public string NodeTypeAlias { get; private set; }
            public string WriterName { get; private set; }
            public string CreatorName { get; private set; }
            public int WriterID { get; private set; }
            public int CreatorID { get; private set; }
            public string Path { get; private set; }
            public DateTime CreateDate { get; private set; }
            public DateTime UpdateDate { get; private set; }
            public Guid Version { get; private set; }

            public string NiceUrl
            {
                get { return _doc.Url; }
            }

            public string Url
            {
                get { return _doc.Url; }
            }

            public int Level { get; private set; }
            public List<IProperty> PropertiesAsList
            {
                get { return _doc.Properties.Select(ConvertToNodeProperty).ToList(); }
            }
            public List<INode> ChildrenAsList
            {
                get { return _doc.Children.Select(ConvertToNode).ToList(); }
            }
            public IProperty GetProperty(string alias)
            {
                return PropertiesAsList.Cast<global::umbraco.NodeFactory.Property>().FirstOrDefault(p => p.Alias == alias);
            }

            public IProperty GetProperty(string alias, out bool propertyExists)
            {
                var prop = _doc.GetProperty(alias);
                propertyExists = prop != null;
                return prop == null ? null : ConvertToNodeProperty(prop);
            }

            public DataTable ChildrenAsTable()
            {
                return _doc.ChildrenAsTable();
            }

            public DataTable ChildrenAsTable(string nodeTypeAliasFilter)
            {
                return _doc.ChildrenAsTable(nodeTypeAliasFilter);
            }
        }

        private class ConvertedProperty : IProperty, IHtmlString
        {
            private readonly IPublishedProperty _prop;

            public ConvertedProperty(IPublishedProperty prop)
            {
                _prop = prop;
            }

            public string Alias
            {
                get { return _prop.PropertyTypeAlias; }
            }

            public string Value
            {
                get { return _prop.DataValue == null ? null : _prop.DataValue.ToString(); }
            }

            public Guid Version
            {
                get { return Guid.Empty; }
            }

            public bool IsNull()
            {
                return Value == null;
            }

            public bool HasValue()
            {
                return _prop.HasValue;
            }

            public int ContextId { get; set; }
            public string ContextAlias { get; set; }

            // implements IHtmlString.ToHtmlString
            public string ToHtmlString()
            {
                return Value;
            }
        }
    }
}
