using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{

	/// <summary>
	/// Represents an IPublishedContent which is created based on an Xml structure.
	/// </summary>
	[Serializable]
	[XmlType(Namespace = "http://umbraco.org/webservices/")]
	internal class XmlPublishedContent : PublishedContentWithKeyBase
	{
		/// <summary>
		/// Initializes a new instance of the <c>XmlPublishedContent</c> class with an Xml node.
		/// </summary>
		/// <param name="xmlNode">The Xml node.</param>
        /// <param name="isPreviewing">A value indicating whether the published content is being previewed.</param>
        private XmlPublishedContent(XmlNode xmlNode, bool isPreviewing)
		{
			_xmlNode = xmlNode;
		    _isPreviewing = isPreviewing;
		}

        private readonly XmlNode _xmlNode;
        private readonly bool _isPreviewing;

        private bool _nodeInitialized;
	    private bool _parentInitialized;
	    private bool _childrenInitialized;

		private IEnumerable<IPublishedContent> _children = Enumerable.Empty<IPublishedContent>();
		private IPublishedContent _parent;

        private PublishedContentType _contentType;
        private Dictionary<string, IPublishedProperty> _properties;

        private int _id;
	    private Guid _key;
		private int _template;
		private string _name;
		private string _docTypeAlias;
		private int _docTypeId;
		private string _writerName;
		private string _creatorName;
		private int _writerId;
		private int _creatorId;
		private string _urlName;
		private string _path;
		private DateTime _createDate;
		private DateTime _updateDate;
		private Guid _version;
		private int _sortOrder;
		private int _level;
	    private bool _isDraft;

		public override IEnumerable<IPublishedContent> Children
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
                if (_childrenInitialized == false) InitializeChildren();
				return _children;
			}
		}

		public override IPublishedProperty GetProperty(string alias)
		{
            if (_nodeInitialized == false) InitializeNode();
            IPublishedProperty property;
		    return _properties.TryGetValue(alias, out property) ? property : null;
		}

        // override to implement cache
        //   cache at context level, ie once for the whole request
        //   but cache is not shared by requests because we wouldn't know how to clear it
        public override IPublishedProperty GetProperty(string alias, bool recurse)
        {
            if (recurse == false) return GetProperty(alias);

            var cache = UmbracoContextCache.Current;

            if (cache == null)
                return base.GetProperty(alias, true);

            var key = string.Format("RECURSIVE_PROPERTY::{0}::{1}", Id, alias.ToLowerInvariant());
            var value = cache.GetOrAdd(key, k => base.GetProperty(alias, true));
            if (value == null)
                return null;

            var property = value as IPublishedProperty;
            if (property == null)
                throw new InvalidOperationException("Corrupted cache.");

            return property;
        }

		public override PublishedItemType ItemType
		{
			get { return PublishedItemType.Content; }
		}

		public override IPublishedContent Parent
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
                if (_parentInitialized == false) InitializeParent();
				return _parent;
			}
		}

		public override int Id
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _id;
			}
		}

	    public override Guid Key
	    {
	        get
	        {
	            if (_nodeInitialized == false) InitializeNode();
	            return _key;
	        }
	    }

		public override int TemplateId
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _template;
			}
		}

		public override int SortOrder
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _sortOrder;
			}
		}

		public override string Name
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _name;
			}
		}

		public override string DocumentTypeAlias
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _docTypeAlias;
			}
		}

		public override int DocumentTypeId
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _docTypeId;
			}
		}

		public override string WriterName
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _writerName;
			}
		}

		public override string CreatorName
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _creatorName;
			}
		}

		public override int WriterId
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _writerId;
			}
		}

		public override int CreatorId
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _creatorId;
			}
		}

		public override string Path
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _path;
			}
		}

		public override DateTime CreateDate
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _createDate;
			}
		}

		public override DateTime UpdateDate
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _updateDate;
			}
		}

		public override Guid Version
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _version;
			}
		}

		public override string UrlName
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _urlName;
			}
		}

		public override int Level
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _level;
			}
		}

	    public override bool IsDraft
	    {
	        get
	        {
	            if (_nodeInitialized == false) InitializeNode();
	            return _isDraft;
	        }
	    }

		public override ICollection<IPublishedProperty> Properties
		{
			get
			{
				if (_nodeInitialized == false) InitializeNode();
				return _properties.Values;
			}
		}

        public override PublishedContentType ContentType
        {
            get
            {
                if (_nodeInitialized == false) InitializeNode();
                return _contentType;
            }
        }

		private void InitializeParent()
		{
            if (_xmlNode == null) return;

            var parent = _xmlNode.ParentNode;
            if (parent == null) return;

		    if (parent.Name == "node" || (parent.Attributes != null && parent.Attributes.GetNamedItem("isDoc") != null))
		        _parent = Get(parent, _isPreviewing);

            // warn: this is not thread-safe...
            _parentInitialized = true;
		}

		private void InitializeNode()
		{
		    if (_xmlNode == null) return;

		    if (_xmlNode.Attributes != null)
		    {
		        _id = int.Parse(_xmlNode.Attributes.GetNamedItem("id").Value);
                if (_xmlNode.Attributes.GetNamedItem("key") != null) // because, migration
    		        _key = Guid.Parse(_xmlNode.Attributes.GetNamedItem("key").Value);
		        if (_xmlNode.Attributes.GetNamedItem("template") != null)
		            _template = int.Parse(_xmlNode.Attributes.GetNamedItem("template").Value);
		        if (_xmlNode.Attributes.GetNamedItem("sortOrder") != null)
		            _sortOrder = int.Parse(_xmlNode.Attributes.GetNamedItem("sortOrder").Value);
		        if (_xmlNode.Attributes.GetNamedItem("nodeName") != null)
		            _name = _xmlNode.Attributes.GetNamedItem("nodeName").Value;
		        if (_xmlNode.Attributes.GetNamedItem("writerName") != null)
		            _writerName = _xmlNode.Attributes.GetNamedItem("writerName").Value;
		        if (_xmlNode.Attributes.GetNamedItem("urlName") != null)
		            _urlName = _xmlNode.Attributes.GetNamedItem("urlName").Value;
		        // Creatorname is new in 2.1, so published xml might not have it!
		        try
		        {
		            _creatorName = _xmlNode.Attributes.GetNamedItem("creatorName").Value;
		        }
		        catch
		        {
		            _creatorName = _writerName;
		        }

		        //Added the actual userID, as a user cannot be looked up via full name only...
		        if (_xmlNode.Attributes.GetNamedItem("creatorID") != null)
		            _creatorId = int.Parse(_xmlNode.Attributes.GetNamedItem("creatorID").Value);
		        if (_xmlNode.Attributes.GetNamedItem("writerID") != null)
		            _writerId = int.Parse(_xmlNode.Attributes.GetNamedItem("writerID").Value);

                if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
		        {
		            if (_xmlNode.Attributes.GetNamedItem("nodeTypeAlias") != null)
		                _docTypeAlias = _xmlNode.Attributes.GetNamedItem("nodeTypeAlias").Value;
		        }
		        else
		        {
		            _docTypeAlias = _xmlNode.Name;
		        }

		        if (_xmlNode.Attributes.GetNamedItem("nodeType") != null)
		            _docTypeId = int.Parse(_xmlNode.Attributes.GetNamedItem("nodeType").Value);
		        if (_xmlNode.Attributes.GetNamedItem("path") != null)
		            _path = _xmlNode.Attributes.GetNamedItem("path").Value;
		        if (_xmlNode.Attributes.GetNamedItem("version") != null)
		            _version = new Guid(_xmlNode.Attributes.GetNamedItem("version").Value);
		        if (_xmlNode.Attributes.GetNamedItem("createDate") != null)
		            _createDate = DateTime.Parse(_xmlNode.Attributes.GetNamedItem("createDate").Value);
		        if (_xmlNode.Attributes.GetNamedItem("updateDate") != null)
		            _updateDate = DateTime.Parse(_xmlNode.Attributes.GetNamedItem("updateDate").Value);
		        if (_xmlNode.Attributes.GetNamedItem("level") != null)
		            _level = int.Parse(_xmlNode.Attributes.GetNamedItem("level").Value);

                _isDraft = (_xmlNode.Attributes.GetNamedItem("isDraft") != null);
            }

		    // load data
            var dataXPath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "data" : "* [not(@isDoc)]";
		    var nodes = _xmlNode.SelectNodes(dataXPath);

		    _contentType = PublishedContentType.Get(PublishedItemType.Content, _docTypeAlias);

		    var propertyNodes = new Dictionary<string, XmlNode>();
            if (nodes != null)
                foreach (XmlNode n in nodes)
                {
                    var attrs = n.Attributes;
                    if (attrs == null) continue;
                    var alias = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema
                        ? attrs.GetNamedItem("alias").Value
                        : n.Name;
                    propertyNodes[alias.ToLowerInvariant()] = n;
                }

		    _properties = _contentType.PropertyTypes.Select(p =>
		    {
		        XmlNode n;
		        return propertyNodes.TryGetValue(p.PropertyTypeAlias.ToLowerInvariant(), out n)
		            ? new XmlPublishedProperty(p, _isPreviewing, n)
		            : new XmlPublishedProperty(p, _isPreviewing);
		    }).Cast<IPublishedProperty>().ToDictionary(
		        x => x.PropertyTypeAlias,
		        x => x,
		        StringComparer.OrdinalIgnoreCase);

            // warn: this is not thread-safe...
            _nodeInitialized = true;
		}

	    private void InitializeChildren()
	    {
	        if (_xmlNode == null) return;

            // load children
            var childXPath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : "* [@isDoc]";
            var nav = _xmlNode.CreateNavigator();
            var expr = nav.Compile(childXPath);
            //expr.AddSort("@sortOrder", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Number);
            var iterator = nav.Select(expr);

            _children = iterator.Cast<XPathNavigator>()
                .Select(n => Get(((IHasXmlNode) n).GetNode(), _isPreviewing))
                .OrderBy(x => x.SortOrder)
                .ToList();

            // warn: this is not thread-safe
            _childrenInitialized = true;
	    }

        /// <summary>
        /// Gets an IPublishedContent corresponding to an Xml cache node.
        /// </summary>
        /// <param name="node">The Xml node.</param>
        /// <param name="isPreviewing">A value indicating whether we are previewing or not.</param>
        /// <returns>The IPublishedContent corresponding to the Xml cache node.</returns>
        /// <remarks>Maintains a per-request cache of IPublishedContent items in order to make
        /// sure that we create only one instance of each for the duration of a request. The
        /// returned IPublishedContent is a model, if models are enabled.</remarks>
        public static IPublishedContent Get(XmlNode node, bool isPreviewing)
        {
            // only 1 per request

            var attrs = node.Attributes;
            var id = attrs == null ? null : attrs.GetNamedItem("id").Value;
            if (id.IsNullOrWhiteSpace()) throw new InvalidOperationException("Node has no ID attribute.");
            var cache = ApplicationContext.Current.ApplicationCache.RequestCache;
            var key = "XMLPUBLISHEDCONTENT_" + id; // dont bother with preview, wont change during request in v7
            return (IPublishedContent) cache.GetCacheItem(key, () => (new XmlPublishedContent(node, isPreviewing)).CreateModel());
        }
    }
}