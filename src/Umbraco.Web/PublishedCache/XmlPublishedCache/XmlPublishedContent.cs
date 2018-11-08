using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using umbraco;
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

	    private readonly object _initializeLock = new object();

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
                EnsureNodeInitialized(andChildren: true);
				return _children;
			}
		}

		public override IPublishedProperty GetProperty(string alias)
		{
            EnsureNodeInitialized();
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
                EnsureNodeInitialized(andParent: true);
				return _parent;
			}
		}

		public override int Id
		{
			get
			{
				EnsureNodeInitialized();
				return _id;
			}
		}

	    public override Guid Key
	    {
	        get
	        {
	            EnsureNodeInitialized();
	            return _key;
	        }
	    }

		public override int TemplateId
		{
			get
			{
				EnsureNodeInitialized();
				return _template;
			}
		}

		public override int SortOrder
		{
			get
			{
				EnsureNodeInitialized();
				return _sortOrder;
			}
		}

		public override string Name
		{
			get
			{
				EnsureNodeInitialized();
				return _name;
			}
		}

		public override string DocumentTypeAlias
		{
			get
			{
				EnsureNodeInitialized();
				return _docTypeAlias;
			}
		}

		public override int DocumentTypeId
		{
			get
			{
				EnsureNodeInitialized();
				return _docTypeId;
			}
		}

		public override string WriterName
		{
			get
			{
				EnsureNodeInitialized();
				return _writerName;
			}
		}

		public override string CreatorName
		{
			get
			{
				EnsureNodeInitialized();
				return _creatorName;
			}
		}

		public override int WriterId
		{
			get
			{
				EnsureNodeInitialized();
				return _writerId;
			}
		}

		public override int CreatorId
		{
			get
			{
				EnsureNodeInitialized();
				return _creatorId;
			}
		}

		public override string Path
		{
			get
			{
				EnsureNodeInitialized();
				return _path;
			}
		}

		public override DateTime CreateDate
		{
			get
			{
				EnsureNodeInitialized();
				return _createDate;
			}
		}

		public override DateTime UpdateDate
		{
			get
			{
				EnsureNodeInitialized();
				return _updateDate;
			}
		}

		public override Guid Version
		{
			get
			{
				EnsureNodeInitialized();
				return _version;
			}
		}

		public override string UrlName
		{
			get
			{
				EnsureNodeInitialized();
				return _urlName;
			}
		}

		public override int Level
		{
			get
			{
				EnsureNodeInitialized();
				return _level;
			}
		}

	    public override bool IsDraft
	    {
	        get
	        {
	            EnsureNodeInitialized();
	            return _isDraft;
	        }
	    }

		public override ICollection<IPublishedProperty> Properties
		{
			get
			{
				EnsureNodeInitialized();
				return _properties.Values;
			}
		}

        public override PublishedContentType ContentType
        {
            get
            {
                EnsureNodeInitialized();
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

            _parentInitialized = true;
		}

	    private void EnsureNodeInitialized(bool andChildren = false, bool andParent = false)
	    {
            // In *theory* XmlPublishedContent are a per-request thing, and so should not
            // end up being involved into multi-threaded situations - however, it's been
            // reported that some users ended up seeing 100% CPU due to infinite loops in
            // the properties dictionary in InitializeNode, which would indicate that the
            // dictionary *is* indeed involved in some multi-threaded operation. No idea
            // what users are doing that cause this, but let's be friendly and use a true
            // lock around initialization.

	        lock (_initializeLock)
	        {
	            if (_nodeInitialized == false) InitializeNode();
                if (andChildren && _childrenInitialized == false) InitializeChildren();
	            if (andParent && _parentInitialized == false) InitializeParent();
	        }
	    }

	    private void InitializeNode()
	    {
            InitializeNode(_xmlNode, UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema, _isPreviewing,
                out _id, out _key, out _template, out _sortOrder, out _name, out _writerName,
                out _urlName, out _creatorName, out _creatorId, out _writerId, out _docTypeAlias, out _docTypeId, out _path,
                out _version, out _createDate, out _updateDate, out _level, out _isDraft, out _contentType, out _properties,
                PublishedContentType.Get);

            _nodeInitialized = true;
        }

        // internal for some benchmarks
        internal static void InitializeNode(XmlNode xmlNode, bool legacy, bool isPreviewing,
            out int id, out Guid key, out int template, out int sortOrder, out string name, out string writerName, out string urlName,
            out string creatorName, out int creatorId, out int writerId, out string docTypeAlias, out int docTypeId, out string path,
            out Guid version, out DateTime createDate, out DateTime updateDate, out int level, out bool isDraft,
            out PublishedContentType contentType, out Dictionary<string, IPublishedProperty> properties,
            Func<PublishedItemType, string, PublishedContentType> getPublishedContentType)
        {
            //initialize the out params with defaults:
            writerName = null;
            docTypeAlias = null;
            id = template = sortOrder = template = creatorId = writerId = docTypeId = level = default(int);
            key = version = default(Guid);
            name = writerName = urlName = creatorName = docTypeAlias = path = null;
            createDate = updateDate = default(DateTime);
            isDraft = false;
            contentType = null;
            properties = null;

            //return if this is null
            if (xmlNode == null)
            {
		        return;
		    }

		    if (xmlNode.Attributes != null)
		    {
		        id = int.Parse(xmlNode.Attributes.GetNamedItem("id").Value);
                if (xmlNode.Attributes.GetNamedItem("key") != null) // because, migration
    		        key = Guid.Parse(xmlNode.Attributes.GetNamedItem("key").Value);
		        if (xmlNode.Attributes.GetNamedItem("template") != null)
		            template = int.Parse(xmlNode.Attributes.GetNamedItem("template").Value);
		        if (xmlNode.Attributes.GetNamedItem("sortOrder") != null)
		            sortOrder = int.Parse(xmlNode.Attributes.GetNamedItem("sortOrder").Value);
		        if (xmlNode.Attributes.GetNamedItem("nodeName") != null)
		            name = xmlNode.Attributes.GetNamedItem("nodeName").Value;
		        if (xmlNode.Attributes.GetNamedItem("writerName") != null)
		            writerName = xmlNode.Attributes.GetNamedItem("writerName").Value;
		        if (xmlNode.Attributes.GetNamedItem("urlName") != null)
		            urlName = xmlNode.Attributes.GetNamedItem("urlName").Value;
		        // Creatorname is new in 2.1, so published xml might not have it!
		        try
		        {
		            creatorName = xmlNode.Attributes.GetNamedItem("creatorName").Value;
		        }
		        catch
		        {
		            creatorName = writerName;
		        }

		        //Added the actual userID, as a user cannot be looked up via full name only...
		        if (xmlNode.Attributes.GetNamedItem("creatorID") != null)
		            creatorId = int.Parse(xmlNode.Attributes.GetNamedItem("creatorID").Value);
		        if (xmlNode.Attributes.GetNamedItem("writerID") != null)
		            writerId = int.Parse(xmlNode.Attributes.GetNamedItem("writerID").Value);

                if (legacy)
		        {
		            if (xmlNode.Attributes.GetNamedItem("nodeTypeAlias") != null)
		                docTypeAlias = xmlNode.Attributes.GetNamedItem("nodeTypeAlias").Value;
		        }
		        else
		        {
		            docTypeAlias = xmlNode.Name;
		        }

		        if (xmlNode.Attributes.GetNamedItem("nodeType") != null)
		            docTypeId = int.Parse(xmlNode.Attributes.GetNamedItem("nodeType").Value);
		        if (xmlNode.Attributes.GetNamedItem("path") != null)
		            path = xmlNode.Attributes.GetNamedItem("path").Value;
		        if (xmlNode.Attributes.GetNamedItem("version") != null)
		            version = new Guid(xmlNode.Attributes.GetNamedItem("version").Value);
		        if (xmlNode.Attributes.GetNamedItem("createDate") != null)
		            createDate = DateTime.Parse(xmlNode.Attributes.GetNamedItem("createDate").Value);
		        if (xmlNode.Attributes.GetNamedItem("updateDate") != null)
		            updateDate = DateTime.Parse(xmlNode.Attributes.GetNamedItem("updateDate").Value);
		        if (xmlNode.Attributes.GetNamedItem("level") != null)
		            level = int.Parse(xmlNode.Attributes.GetNamedItem("level").Value);

                isDraft = (xmlNode.Attributes.GetNamedItem("isDraft") != null);
            }

		    //dictionary to store the property node data
            var propertyNodes = new Dictionary<string, XmlNode>();

            foreach (XmlNode n in xmlNode.ChildNodes)
            {
                var e = n as XmlElement;
                if (e == null) continue;
		        if (legacy)
		        {
		            if (n.Name == "data")
		            {
		                PopulatePropertyNodes(propertyNodes, e, true);
		            }
		            else break; //we are not longer on property elements
                }
		        else
		        {
		            if (e.HasAttribute("isDoc") == false)
		            {
                        PopulatePropertyNodes(propertyNodes, e, false);
		            }
		            else break; //we are not longer on property elements
		        }
		    }

            //lookup the content type and create the properties collection
            try
            {
                contentType = getPublishedContentType(PublishedItemType.Content, docTypeAlias);

            }
            catch (InvalidOperationException e)
            {
                content.Instance.RefreshContentFromDatabase();
                

                throw new InvalidOperationException(
                    string.Format("{0}. This usually indicates that the content cache is corrupt; the content cache has been rebuilt in an attempt to self-fix the issue.", 
                    //keep the original message but don't use this as an inner exception because we want the above message to be displayed, if we use the inner exception
                    //we can keep the stack trace but the header message will be the original message and the one we really want to display will be hidden below the fold.
                    e.Message));
            }
            properties = new Dictionary<string, IPublishedProperty>(StringComparer.OrdinalIgnoreCase);

            //fill in the property collection
		    foreach (var propertyType in contentType.PropertyTypes)
		    {
                XmlNode n;
		        var val = propertyNodes.TryGetValue(propertyType.PropertyTypeAlias.ToLowerInvariant(), out n)
		            ? new XmlPublishedProperty(propertyType, isPreviewing, n)
		            : new XmlPublishedProperty(propertyType, isPreviewing);

		        properties[propertyType.PropertyTypeAlias] = val;
		    }
		}

        private static void PopulatePropertyNodes(IDictionary<string, XmlNode> propertyNodes, XmlNode n, bool legacy)
        {
            var attrs = n.Attributes;
	        if (attrs == null) return;

            var alias = legacy
                ? attrs.GetNamedItem("alias").Value
                : n.Name;
            propertyNodes[alias.ToLowerInvariant()] = n;
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
            var key = CacheKeyPrefix + id; // dont bother with preview, wont change during request in v7
            return (IPublishedContent) cache.GetCacheItem(key, () => (new XmlPublishedContent(node, isPreviewing)).CreateModel());
        }

	    public static void ClearRequest()
	    {
	        ApplicationContext.Current.ApplicationCache.RequestCache.ClearCacheByKeySearch(CacheKeyPrefix);
	    }

	    private const string CacheKeyPrefix = "CONTENTCACHE_XMLPUBLISHEDCONTENT_";
	}
}
