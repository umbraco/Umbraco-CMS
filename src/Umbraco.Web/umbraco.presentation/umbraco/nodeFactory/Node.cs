using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.propertytype;
using umbraco.interfaces;
using Umbraco.Core;

namespace umbraco.NodeFactory
{
	/// <summary>
	/// Summary description for Node.
	/// </summary>

	[Serializable]
	[XmlType(Namespace = "http://umbraco.org/webservices/")]
	public class Node : INode
	{
		private Hashtable _aliasToNames = new Hashtable();

		private bool _initialized = false;
		private Nodes _children = new Nodes();
		private Node _parent = null;
		private int _id;
		private int _template;
		private string _name;
		private string _nodeTypeAlias;
		private string _writerName;
		private string _creatorName;
		private int _writerID;
		private int _creatorID;
		private string _urlName;
		private string _path;
		private DateTime _createDate;
		private DateTime _updateDate;
		private Guid _version;
		private Properties _properties = new Properties();
		private XmlNode _pageXmlNode;
		private int _sortOrder;
		private int _level;

		public Nodes Children
		{
			get
			{
				if (!_initialized)
					initialize();
				return _children;
			}
		}

		public INode Parent
		{
			get
			{
				if (!_initialized)
					initialize();
				return _parent;
			}
		}

		public int Id
		{
			get
			{
				if (!_initialized)
					initialize();
				return _id;
			}
		}

		public int template
		{
			get
			{
				if (!_initialized)
					initialize();
				return _template;
			}
		}

		public int SortOrder
		{
			get
			{
				if (!_initialized)
					initialize();
				return _sortOrder;
			}
		}

		public string Name
		{
			get
			{
				if (!_initialized)
					initialize();
				return _name;
			}
		}

		public string Url
		{
			get { return library.NiceUrl(Id); }
		}

		public string NodeTypeAlias
		{
			get
			{
				if (!_initialized)
					initialize();
				return _nodeTypeAlias;
			}
		}

		public string WriterName
		{
			get
			{
				if (!_initialized)
					initialize();
				return _writerName;
			}
		}

		public string CreatorName
		{
			get
			{
				if (!_initialized)
					initialize();
				return _creatorName;
			}
		}

		public int WriterID
		{
			get
			{
				if (!_initialized)
					initialize();
				return _writerID;
			}
		}

		public int CreatorID
		{
			get
			{
				if (!_initialized)
					initialize();
				return _creatorID;
			}
		}


		public string Path
		{
			get
			{
				if (!_initialized)
					initialize();
				return _path;
			}
		}

		public DateTime CreateDate
		{
			get
			{
				if (!_initialized)
					initialize();
				return _createDate;
			}
		}

		public DateTime UpdateDate
		{
			get
			{
				if (!_initialized)
					initialize();
				return _updateDate;
			}
		}

		public Guid Version
		{
			get
			{
				if (!_initialized)
					initialize();
				return _version;
			}
		}

		public string UrlName
		{
			get
			{
				if (!_initialized)
					initialize();
				return _urlName;
			}
		}

		public string NiceUrl
		{
			get
			{
				return library.NiceUrl(_id);
			}
		}

		public int Level
		{
			get
			{
				if (!_initialized)
					initialize();
				return _level;
			}
		}

		public List<IProperty> PropertiesAsList
		{
			get { return Properties.Cast<IProperty>().ToList(); }
		}

		public Properties Properties
		{
			get
			{
				if (!_initialized)
					initialize();
				return _properties;
			}
		}

		public Node()
		{
			_pageXmlNode = ((IHasXmlNode)library.GetXmlNodeCurrent().Current).GetNode();
			initializeStructure();
			initialize();
		}

		public Node(XmlNode NodeXmlNode)
		{
			_pageXmlNode = NodeXmlNode;
			initializeStructure();
			initialize();
		}

		public Node(XmlNode NodeXmlNode, bool DisableInitializing)
		{
			_pageXmlNode = NodeXmlNode;
			initializeStructure();
			if (!DisableInitializing)
				initialize();
		}

		/// <summary>
		/// Special constructor for by-passing published vs. preview xml to use
		/// when updating the SiteMapProvider
		/// </summary>
		/// <param name="NodeId"></param>
		/// <param name="forcePublishedXml"></param>
		public Node(int NodeId, bool forcePublishedXml)
		{
			if (forcePublishedXml)
			{
				if (NodeId != -1)
					_pageXmlNode = content.Instance.XmlContent.GetElementById(NodeId.ToString());
				else
				{
					_pageXmlNode = content.Instance.XmlContent.DocumentElement;

				}
				initializeStructure();
				initialize();
			}
			else
			{
				throw new ArgumentException("Use Node(int NodeId) if not forcing published xml");

			}
		}

		public Node(int NodeId)
		{
			if (NodeId != -1)
				_pageXmlNode = ((IHasXmlNode)library.GetXmlNodeById(NodeId.ToString()).Current).GetNode();
			else
			{
				if (presentation.UmbracoContext.Current != null)
				{
					_pageXmlNode = umbraco.presentation.UmbracoContext.Current.GetXml().DocumentElement;
				}
				else
				{
					_pageXmlNode = content.Instance.XmlContent.DocumentElement;
				}


			}
			initializeStructure();
			initialize();
		}

		public IProperty GetProperty(string Alias)
		{
			foreach (Property p in Properties)
			{
				if (p.Alias == Alias)
					return p;
			}
			return null;
		}

		public IProperty GetProperty(string Alias, out bool propertyExists)
		{
			foreach (Property p in Properties)
			{
				if (p.Alias == Alias)
				{
					propertyExists = true;
					return p;
				}
			}
			propertyExists = false;
			return null;
		}

		public static Node GetNodeByXpath(string xpath)
		{
			XPathNodeIterator itNode = library.GetXmlNodeByXPath(xpath);
			XmlNode nodeXmlNode = null;
			if (itNode.MoveNext())
			{
				nodeXmlNode = ((IHasXmlNode)itNode.Current).GetNode();
			}
			if (nodeXmlNode != null)
			{
				return new Node(nodeXmlNode);
			}
			return null;
		}

		public List<INode> ChildrenAsList
		{
			get { return Children.Cast<INode>().ToList(); }
		}

		public DataTable ChildrenAsTable()
		{
			return Children.Count > 0 ? GenerateDataTable(Children[0]) : new DataTable();
		}

		public DataTable ChildrenAsTable(string nodeTypeAliasFilter)
		{
			if (Children.Count > 0)
			{

				Node Firstnode = null;
				Boolean nodeFound = false;
				foreach (Node n in Children)
				{
					if (n.NodeTypeAlias == nodeTypeAliasFilter && !nodeFound)
					{
						Firstnode = n;
						nodeFound = true;
						break;
					}
				}

				if (nodeFound)
				{
					return GenerateDataTable(Firstnode);
				}
				return new DataTable();
			}
			return new DataTable();
		}

		private DataTable GenerateDataTable(INode node)
		{
			//use new utility class to create table so that we don't have to maintain code in many places, just one
			var dt = Umbraco.Core.DataTableExtensions.GenerateDataTable(
				//pass in the alias
				node.NodeTypeAlias,				
				//pass in the callback to extract the Dictionary<string, string> of user defined aliases to their names
				alias =>
					{
						var ct = ContentType.GetByAlias(alias);
						return ct.PropertyTypes.ToDictionary(x => x.Alias, x => x.Name);
					},
				//pass in a callback to populate the datatable, yup its a bit ugly but it's already legacy and we just want to maintain code in one place.
				() =>
					{
						//create all row data
						var tableData = Umbraco.Core.DataTableExtensions.CreateTableData();
						//loop through each child and create row data for it
						foreach (Node n in Children)
						{
							var standardVals = new Dictionary<string, object>()
								{
									{"Id", n.Id},
									{"NodeName", n.Name},
									{"NodeTypeAlias", n.NodeTypeAlias},
									{"CreateDate", n.CreateDate},
									{"UpdateDate", n.UpdateDate},
									{"CreatorName", n.CreatorName},
									{"WriterName", n.WriterName},
									{"Url", library.NiceUrl(n.Id)}
								};
							var userVals = new Dictionary<string, object>();
							foreach (var p in from Property p in n.Properties where p.Value != null select p)
							{
								userVals[p.Alias] = p.Value;
							}
							//add the row data
							Umbraco.Core.DataTableExtensions.AddRowData(tableData, standardVals, userVals);
						}
						return tableData;
					}
				);
			return dt;
		}

		private void initializeStructure()
		{
			// Load parent if it exists and is a node

			if (_pageXmlNode != null && _pageXmlNode.SelectSingleNode("..") != null)
			{
				XmlNode parent = _pageXmlNode.SelectSingleNode("..");
				if (parent != null && (parent.Name == "node" || (parent.Attributes != null && parent.Attributes.GetNamedItem("isDoc") != null)))
					_parent = new Node(parent, true);
			}
		}

		private void initialize()
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
						_creatorID = int.Parse(_pageXmlNode.Attributes.GetNamedItem("creatorID").Value);
					if (_pageXmlNode.Attributes.GetNamedItem("writerID") != null)
						_writerID = int.Parse(_pageXmlNode.Attributes.GetNamedItem("writerID").Value);

					if (UmbracoSettings.UseLegacyXmlSchema)
					{
						if (_pageXmlNode.Attributes.GetNamedItem("nodeTypeAlias") != null)
							_nodeTypeAlias = _pageXmlNode.Attributes.GetNamedItem("nodeTypeAlias").Value;
					}
					else
					{
						_nodeTypeAlias = _pageXmlNode.Name;
					}

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
				string dataXPath = UmbracoSettings.UseLegacyXmlSchema ? "data" : "* [not(@isDoc)]";
				foreach (XmlNode n in _pageXmlNode.SelectNodes(dataXPath))
					_properties.Add(new Property(n));

				// load children
				string childXPath = UmbracoSettings.UseLegacyXmlSchema ? "node" : "* [@isDoc]";
				XPathNavigator nav = _pageXmlNode.CreateNavigator();
				XPathExpression expr = nav.Compile(childXPath);
				expr.AddSort("@sortOrder", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Number);
				XPathNodeIterator iterator = nav.Select(expr);
				while (iterator.MoveNext())
				{
					_children.Add(
						new Node(((IHasXmlNode)iterator.Current).GetNode(), true)
						);
				}
			}
			//            else
			//                throw new ArgumentNullException("Node xml source is null");
		}

		public static Node GetCurrent()
		{
			int id = getCurrentNodeId();
			return new Node(id);
		}

		public static int getCurrentNodeId()
		{
			XmlNode n = ((IHasXmlNode)library.GetXmlNodeCurrent().Current).GetNode();
			if (n.Attributes == null || n.Attributes.GetNamedItem("id") == null)
				throw new ArgumentException("Current node is null. This might be due to previewing an unpublished node. As the NodeFactory works with published data, macros using the node factory won't work in preview mode.", "Current node is " + System.Web.HttpContext.Current.Items["pageID"].ToString());

			return int.Parse(n.Attributes.GetNamedItem("id").Value);
		}
	}
}