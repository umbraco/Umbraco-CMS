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
using Umbraco.Web.Routing;

namespace Umbraco.Web.Models
{

	/// <summary>
	/// Represents an IPublishedContent which is created based on an Xml structure
	/// </summary>
	[Serializable]
	[XmlType(Namespace = "http://umbraco.org/webservices/")]
	internal class XmlPublishedContent : PublishedContentBase
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="xmlNode"></param>
		public XmlPublishedContent(XmlNode xmlNode)
		{
			_pageXmlNode = xmlNode;
			InitializeStructure();
			Initialize();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <param name="disableInitializing"></param>
		internal XmlPublishedContent(XmlNode xmlNode, bool disableInitializing)
		{
			_pageXmlNode = xmlNode;
			InitializeStructure();
			if (!disableInitializing)
				Initialize();
		}

		private bool _initialized = false;
		private readonly ICollection<IPublishedContent> _children = new Collection<IPublishedContent>();
		private IPublishedContent _parent = null;
		private int _id;
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
		private readonly Collection<IPublishedContentProperty> _properties = new Collection<IPublishedContentProperty>();
		private readonly XmlNode _pageXmlNode;
		private int _sortOrder;
		private int _level;

		public override IEnumerable<IPublishedContent> Children
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _children.OrderBy(x => x.SortOrder);
			}
		}

		public override IPublishedContentProperty GetProperty(string alias)
		{
			return Properties.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
		}

		/// <summary>
		/// returns 'Content' as the ItemType
		/// </summary>
		public override PublishedItemType ItemType
		{
			get { return PublishedItemType.Content; }
		}

		public override IPublishedContent Parent
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _parent;
			}
		}

		public override int Id
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _id;
			}
		}

		public override int TemplateId
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _template;
			}
		}

		public override int SortOrder
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _sortOrder;
			}
		}

		public override string Name
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _name;
			}
		}

		public override string DocumentTypeAlias
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _docTypeAlias;
			}
		}

		public override int DocumentTypeId
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _docTypeId;
			}
		}

		public override string WriterName
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _writerName;
			}
		}

		public override string CreatorName
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _creatorName;
			}
		}

		public override int WriterId
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _writerId;
			}
		}

		public override int CreatorId
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _creatorId;
			}
		}


		public override string Path
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _path;
			}
		}

		public override DateTime CreateDate
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _createDate;
			}
		}

		public override DateTime UpdateDate
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _updateDate;
			}
		}

		public override Guid Version
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _version;
			}
		}

		public override string UrlName
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _urlName;
			}
		}

		public override int Level
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _level;
			}
		}

		public override ICollection<IPublishedContentProperty> Properties
		{
			get
			{
				if (!_initialized)
					Initialize();
				return _properties;
			}
		}
		

		private void InitializeStructure()
		{
			// Load parent if it exists and is a node

			if (_pageXmlNode != null && _pageXmlNode.SelectSingleNode("..") != null)
			{
				XmlNode parent = _pageXmlNode.SelectSingleNode("..");
				if (parent != null && (parent.Name == "node" || (parent.Attributes != null && parent.Attributes.GetNamedItem("isDoc") != null)))
					_parent = new XmlPublishedContent(parent, true);
			}
		}

		private void Initialize()
		{
			if (_pageXmlNode != null)
			{
				_initialized = true;
				if (_pageXmlNode.Attributes != null)
				{
					_id = int.Parse(_pageXmlNode.Attributes.GetNamedItem("id").Value);
					if (_pageXmlNode.Attributes.GetNamedItem("template") != null)
						_template = int.Parse(_pageXmlNode.Attributes.GetNamedItem("template").Value);
					if (_pageXmlNode.Attributes.GetNamedItem("sortOrder") != null)
						_sortOrder = int.Parse(_pageXmlNode.Attributes.GetNamedItem("sortOrder").Value);
					if (_pageXmlNode.Attributes.GetNamedItem("nodeName") != null)
						_name = _pageXmlNode.Attributes.GetNamedItem("nodeName").Value;
					if (_pageXmlNode.Attributes.GetNamedItem("writerName") != null)
						_writerName = _pageXmlNode.Attributes.GetNamedItem("writerName").Value;
					if (_pageXmlNode.Attributes.GetNamedItem("urlName") != null)
						_urlName = _pageXmlNode.Attributes.GetNamedItem("urlName").Value;
					// Creatorname is new in 2.1, so published xml might not have it!
					try
					{
						_creatorName = _pageXmlNode.Attributes.GetNamedItem("creatorName").Value;
					}
					catch
					{
						_creatorName = _writerName;
					}

					//Added the actual userID, as a user cannot be looked up via full name only... 
					if (_pageXmlNode.Attributes.GetNamedItem("creatorID") != null)
						_creatorId = int.Parse(_pageXmlNode.Attributes.GetNamedItem("creatorID").Value);
					if (_pageXmlNode.Attributes.GetNamedItem("writerID") != null)
						_writerId = int.Parse(_pageXmlNode.Attributes.GetNamedItem("writerID").Value);

					if (LegacyUmbracoSettings.UseLegacyXmlSchema)
					{
						if (_pageXmlNode.Attributes.GetNamedItem("nodeTypeAlias") != null)
							_docTypeAlias = _pageXmlNode.Attributes.GetNamedItem("nodeTypeAlias").Value;
					}
					else
					{
						_docTypeAlias = _pageXmlNode.Name;
					}

					if (_pageXmlNode.Attributes.GetNamedItem("nodeType") != null)
						_docTypeId = int.Parse(_pageXmlNode.Attributes.GetNamedItem("nodeType").Value);
					if (_pageXmlNode.Attributes.GetNamedItem("path") != null)
						_path = _pageXmlNode.Attributes.GetNamedItem("path").Value;
					if (_pageXmlNode.Attributes.GetNamedItem("version") != null)
						_version = new Guid(_pageXmlNode.Attributes.GetNamedItem("version").Value);
					if (_pageXmlNode.Attributes.GetNamedItem("createDate") != null)
						_createDate = DateTime.Parse(_pageXmlNode.Attributes.GetNamedItem("createDate").Value);
					if (_pageXmlNode.Attributes.GetNamedItem("updateDate") != null)
						_updateDate = DateTime.Parse(_pageXmlNode.Attributes.GetNamedItem("updateDate").Value);
					if (_pageXmlNode.Attributes.GetNamedItem("level") != null)
						_level = int.Parse(_pageXmlNode.Attributes.GetNamedItem("level").Value);

				}

				// load data
				var dataXPath = LegacyUmbracoSettings.UseLegacyXmlSchema ? "data" : "* [not(@isDoc)]";
				foreach (XmlNode n in _pageXmlNode.SelectNodes(dataXPath))
					_properties.Add(new XmlPublishedContentProperty(n));

				// load children
				var childXPath = LegacyUmbracoSettings.UseLegacyXmlSchema ? "node" : "* [@isDoc]";
				var nav = _pageXmlNode.CreateNavigator();
				var expr = nav.Compile(childXPath);
				expr.AddSort("@sortOrder", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Number);
				var iterator = nav.Select(expr);
				while (iterator.MoveNext())
				{
					_children.Add(
						new XmlPublishedContent(((IHasXmlNode)iterator.Current).GetNode(), true)
						);
				}
			}
			//            else
			//                throw new ArgumentNullException("Node xml source is null");
		}

	}
}