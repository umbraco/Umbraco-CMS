using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public XmlPublishedContent(XmlNode xmlNode, bool isPreviewing)
		{
			_xmlNode = xmlNode;
		    _isPreviewing = isPreviewing;
			InitializeStructure();
			Initialize();
            InitializeChildren();
		}

        /// <summary>
        /// Initializes a new instance of the <c>XmlPublishedContent</c> class with an Xml node,
        /// and a value indicating whether to lazy-initialize the instance.
        /// </summary>
        /// <param name="xmlNode">The Xml node.</param>
        /// <param name="isPreviewing">A value indicating whether the published content is being previewed.</param>
        /// <param name="lazyInitialize">A value indicating whether to lazy-initialize the instance.</param>
        /// <remarks>Lazy-initializationg is NOT thread-safe.</remarks>
        internal XmlPublishedContent(XmlNode xmlNode, bool isPreviewing, bool lazyInitialize)
		{
			_xmlNode = xmlNode;
            _isPreviewing = isPreviewing;
			InitializeStructure();
            if (lazyInitialize == false)
            {
                Initialize();
                InitializeChildren();
            }
		}

        private readonly XmlNode _xmlNode;
        
        private bool _initialized;
	    private bool _childrenInitialized;

		private readonly ICollection<IPublishedContent> _children = new Collection<IPublishedContent>();
		private IPublishedContent _parent;

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
	    private IPublishedProperty[] _properties;
		private int _sortOrder;
		private int _level;
	    private bool _isDraft;
	    private readonly bool _isPreviewing;
	    private PublishedContentType _contentType;

		public override IEnumerable<IPublishedContent> Children
		{
			get
			{
				if (_initialized == false)
					Initialize();
                if (_childrenInitialized == false)
                    InitializeChildren();
				return _children.OrderBy(x => x.SortOrder);
			}
		}

		public override IPublishedProperty GetProperty(string alias)
		{
			return Properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
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
				if (_initialized == false)
					Initialize();
				return _parent;
			}
		}

		public override int Id
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _id;
			}
		}

	    public override Guid Key
	    {
	        get
	        {
	            if (_initialized == false)
                    Initialize();
	            return _key;
	        }
	    }

		public override int TemplateId
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _template;
			}
		}

		public override int SortOrder
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _sortOrder;
			}
		}

		public override string Name
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _name;
			}
		}

		public override string DocumentTypeAlias
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _docTypeAlias;
			}
		}

		public override int DocumentTypeId
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _docTypeId;
			}
		}

		public override string WriterName
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _writerName;
			}
		}

		public override string CreatorName
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _creatorName;
			}
		}

		public override int WriterId
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _writerId;
			}
		}

		public override int CreatorId
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _creatorId;
			}
		}

		public override string Path
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _path;
			}
		}

		public override DateTime CreateDate
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _createDate;
			}
		}

		public override DateTime UpdateDate
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _updateDate;
			}
		}

		public override Guid Version
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _version;
			}
		}

		public override string UrlName
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _urlName;
			}
		}

		public override int Level
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _level;
			}
		}

	    public override bool IsDraft
	    {
	        get
	        {
	            if (_initialized == false)
                    Initialize();
	            return _isDraft;
	        }
	    }

		public override ICollection<IPublishedProperty> Properties
		{
			get
			{
				if (_initialized == false)
					Initialize();
				return _properties;
			}
		}

        public override PublishedContentType ContentType
        {
            get
            {
                if (_initialized == false)
                    Initialize();
                return _contentType;
            }
        }
		
		private void InitializeStructure()
		{
			// load parent if it exists and is a node

		    var parent = _xmlNode == null ? null : _xmlNode.ParentNode;            
            if (parent == null) return;

		    if (parent.Name == "node" || (parent.Attributes != null && parent.Attributes.GetNamedItem("isDoc") != null))
		        _parent = (new XmlPublishedContent(parent, _isPreviewing, true)).CreateModel();
		}

		private void Initialize()
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
                    var alias = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema
                        ? n.Attributes.GetNamedItem("alias").Value
                        : n.Name;
                    propertyNodes[alias.ToLowerInvariant()] = n;
                }

            _properties = _contentType.PropertyTypes.Select(p =>
		        {
                    XmlNode n;
                    return propertyNodes.TryGetValue(p.PropertyTypeAlias.ToLowerInvariant(), out n)
                        ? new XmlPublishedProperty(p, _isPreviewing, n)
                        : new XmlPublishedProperty(p, _isPreviewing);		        
		        }).Cast<IPublishedProperty>().ToArray();

            // warn: this is not thread-safe...
            _initialized = true;
		}

	    private void InitializeChildren()
	    {
	        if (_xmlNode == null) return;

            // load children
            var childXPath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : "* [@isDoc]";
            var nav = _xmlNode.CreateNavigator();
            var expr = nav.Compile(childXPath);
            expr.AddSort("@sortOrder", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Number);
            var iterator = nav.Select(expr);
            while (iterator.MoveNext())
		        _children.Add(
                    (new XmlPublishedContent(((IHasXmlNode)iterator.Current).GetNode(), _isPreviewing, true)).CreateModel());
            
            // warn: this is not thread-safe
            _childrenInitialized = true;
	    }
    }
}