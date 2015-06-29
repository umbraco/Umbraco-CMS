using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using umbraco.interfaces;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A legacy INode that wraps IPublishedContent
    /// </summary>
    internal class LegacyConvertedNode : PublishedContentWrapped, INode
    {
        private readonly IPublishedContent _publishedContent;
        private readonly int _id;
        private readonly int _template;
        private readonly int _sortOrder;
        private readonly string _name;
        private readonly string _urlName;
        private readonly string _nodeTypeAlias;
        private readonly string _writerName;
        private readonly string _creatorName;
        private readonly int _writerId;
        private readonly int _creatorId;
        private readonly string _path;
        private readonly DateTime _createDate;
        private readonly DateTime _updateDate;
        private readonly Guid _version;
        private readonly int _level;

        public LegacyConvertedNode(IPublishedContent publishedContent) : base(publishedContent)
        {
            _publishedContent = publishedContent;

            if (publishedContent == null)
            {
                _id = 0;
                return;
            }

            _template = publishedContent.TemplateId;
            _id = publishedContent.Id;
            _path = publishedContent.Path;
            _creatorName = publishedContent.CreatorName;
            _sortOrder = publishedContent.SortOrder;
            _updateDate = publishedContent.UpdateDate;
            _name = publishedContent.Name;
            _nodeTypeAlias = publishedContent.DocumentTypeAlias;
            _createDate = publishedContent.CreateDate;
            _creatorId = publishedContent.CreatorId;
            _level = publishedContent.Level;
            _urlName = publishedContent.UrlName;
            _version = publishedContent.Version;
            _writerId = publishedContent.WriterId;
            _writerName = publishedContent.WriterName;
        }

        INode INode.Parent
        {
            get { return _publishedContent.Parent == null ? null : LegacyNodeHelper.ConvertToNode(_publishedContent.Parent); }
        }

        int INode.Id
        {
            get { return _id; }
        }

        int INode.template
        {
            get { return _template; }
        }

        int INode.SortOrder
        {
            get { return _sortOrder; }
        }

        string INode.Name
        {
            get { return _name; }
        }

        string INode.UrlName
        {
            get { return _urlName; }
        }

        string INode.NodeTypeAlias
        {
            get { return _nodeTypeAlias; }
        }

        string INode.WriterName
        {
            get { return _writerName; }
        }

        string INode.CreatorName
        {
            get { return _creatorName; }
        }

        int INode.WriterID
        {
            get { return _writerId; }
        }

        int INode.CreatorID
        {
            get { return _creatorId; }
        }

        string INode.Path
        {
            get { return _path; }
        }

        DateTime INode.CreateDate
        {
            get { return _createDate; }
        }

        DateTime INode.UpdateDate
        {
            get { return _updateDate; }
        }

        Guid INode.Version
        {
            get { return _version; }
        }

        string INode.NiceUrl
        {
            get { return _publishedContent.Url; }
        }

        string INode.Url
        {
            get { return _publishedContent.Url; }
        }

        int INode.Level
        {
            get { return _level; }
        }

        List<IProperty> INode.PropertiesAsList
        {
            get { return _publishedContent.Properties.Select(LegacyNodeHelper.ConvertToNodeProperty).ToList(); }
        }

        List<INode> INode.ChildrenAsList
        {
            get { return _publishedContent.Children.Select(LegacyNodeHelper.ConvertToNode).ToList(); }
        }

        IProperty INode.GetProperty(string alias)
        {
            return ((INode)this).PropertiesAsList.Cast<global::umbraco.NodeFactory.Property>().FirstOrDefault(p => p.Alias == alias);
        }

        IProperty INode.GetProperty(string alias, out bool propertyExists)
        {
            var prop = _publishedContent.GetProperty(alias);
            propertyExists = prop != null;
            return prop == null ? null : LegacyNodeHelper.ConvertToNodeProperty(prop);
        }

        DataTable INode.ChildrenAsTable()
        {
            return _publishedContent.ChildrenAsTable();
        }

        DataTable INode.ChildrenAsTable(string nodeTypeAliasFilter)
        {
            return _publishedContent.ChildrenAsTable(nodeTypeAliasFilter);
        }
    }
}