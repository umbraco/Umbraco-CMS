using System.Xml.Serialization;
using System.Collections;
using System;
using umbraco.BusinessLogic.Utils;
using System.Xml.Schema;
using umbraco.interfaces;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Text;
using umbraco.businesslogic.Utils;
using umbraco.BasePages;
using Umbraco.Core.IO;

namespace umbraco.cms.presentation.Trees
{

	public enum SerializedTreeType
	{
		XmlTree,
		JSONTree,
		JsTree
	}

	/// <summary>
	/// Used for serializing data to XML as the data structure for the JavaScript tree
	/// </summary>
	[XmlRoot(ElementName = "tree", IsNullable = false), Serializable]
	public class XmlTree
	{

		public XmlTree()
		{
			//set to the XTree provider by default.
			//m_TreeType = SerializedTreeType.XmlTree;
			//m_TreeType = SerializedTreeType.JSONTree;			
			m_TreeType = SerializedTreeType.JsTree;

			Init();
		}

		/// <summary>
		/// Use this constructor to force a tree provider to be used
		/// </summary>
		/// <param name="treeType"></param>
		public XmlTree(SerializedTreeType treeType)
		{
			m_TreeType = treeType;
			Init();
		}

		private void Init()
		{
            m_JSSerializer = new JSONSerializer { MaxJsonLength = int.MaxValue };

			switch (m_TreeType)
			{
				case SerializedTreeType.XmlTree:
					break;
				case SerializedTreeType.JSONTree:
					m_JSSerializer.RegisterConverters(new List<JavaScriptConverter>() 
					{ 
						new JSONTreeConverter(),
						new JSONTreeNodeConverter()
					});
					break;
				case SerializedTreeType.JsTree:
					m_JSSerializer.RegisterConverters(new List<JavaScriptConverter>() 
					{ 	
						new JsTreeNodeConverter()
					});
					break;
			}


		}

		private JSONSerializer m_JSSerializer;
		private SerializedTreeType m_TreeType;

		/// <summary>
		/// Returns the string representation of the tree structure depending on the SerializedTreeType
		/// specified. 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ToString(m_TreeType);
		}

		public string ToString(SerializedTreeType type)
		{
			switch (type)
			{
				case SerializedTreeType.XmlTree:
					return SerializableData.Serialize(this, typeof(XmlTree));
				case SerializedTreeType.JsTree:
					return m_JSSerializer.Serialize(this.treeCollection);
				case SerializedTreeType.JSONTree:
					return m_JSSerializer.Serialize(this);
			}
			return "";
		}

		public void Add(XmlTreeNode obj)
		{
			treeCollection.Add(obj);
		}

		[XmlIgnore]
		public XmlTreeNode this[int index]
		{
			get { return (XmlTreeNode)treeCollection[index]; }
		}

		[XmlIgnore]
		public int Count
		{
			get { return treeCollection.Count; }
		}

		public void Clear()
		{
			treeCollection.Clear();
		}

		public XmlTreeNode Remove(int index)
		{
			XmlTreeNode obj = treeCollection[index];
			treeCollection.Remove(obj);
			return obj;
		}

		public void Remove(XmlTreeNode obj)
		{
			treeCollection.Remove(obj);
		}


		private List<XmlTreeNode> __treeCollection;

		[XmlElement(Type = typeof(XmlTreeNode), ElementName = "tree", IsNullable = false, Form = XmlSchemaForm.Qualified)]
		public List<XmlTreeNode> treeCollection
		{
			get
			{
				if (__treeCollection == null) __treeCollection = new List<XmlTreeNode>();
				return __treeCollection;
			}
			set { __treeCollection = value; }
		}

        [System.Runtime.InteropServices.DispIdAttribute(-4)]
        public IEnumerator GetEnumerator()
        {
            return (treeCollection as IEnumerable).GetEnumerator();
        }

    }

	/// <summary>
	/// Used for serializing data to XML as the data structure for the JavaScript tree
	/// </summary>
	[Serializable]
	public class XmlTreeNode : IXmlSerializable
	{

		private XmlTreeNode()
		{
			m_nodeStyle = new NodeStyle();
		}

		/// <summary>
		/// creates a new XmlTreeNode with the default parameters from the BaseTree
		/// </summary>
		/// <param name="bTree"></param>
		/// <returns></returns>
		public static XmlTreeNode Create(BaseTree bTree)
		{
			XmlTreeNode xNode = new XmlTreeNode();
			xNode.Menu = bTree.AllowedActions.FindAll(delegate(IAction a) { return true; }); //return a duplicate copy of the list
			xNode.NodeType = bTree.TreeAlias;
			xNode.Source = string.Empty;
			xNode.IsRoot = false;
			//generally the tree type and node type are the same but in some cased they are not.
			xNode.m_treeType = bTree.TreeAlias;
			return xNode;
		}

		/// <summary>
		/// creates a new XmlTreeNode with the default parameters for the BaseTree root node
		/// </summary>
		/// <param name="bTree"></param>
		/// <returns></returns>
		public static XmlTreeNode CreateRoot(BaseTree bTree)
		{
			XmlTreeNode xNode = new XmlTreeNode();
			xNode.NodeID = bTree.StartNodeID.ToString();
			xNode.Source = bTree.GetTreeServiceUrl();
			xNode.Menu = bTree.RootNodeActions.FindAll(delegate(IAction a) { return true; }); //return a duplicate copy of the list
			xNode.NodeType = bTree.TreeAlias;
			xNode.Text = BaseTree.GetTreeHeader(bTree.TreeAlias);
			
            // By default the action from the trees.config will be used, if none is specified then the apps dashboard will be used.
            var appTreeItem = umbraco.BusinessLogic.ApplicationTree.getByAlias(bTree.TreeAlias);
            xNode.Action = appTreeItem == null || String.IsNullOrEmpty(appTreeItem.Action) ? "javascript:" + ClientTools.Scripts.OpenDashboard(bTree.app) : "javascript:" + appTreeItem.Action;
			
            xNode.IsRoot = true;
			//generally the tree type and node type are the same but in some cased they are not.
			xNode.m_treeType = bTree.TreeAlias;
			return xNode;
		}

		private NodeStyle m_nodeStyle;

		private bool? m_notPublished;
		private bool? m_isProtected;
		private List<IAction> m_menu;
		private string m_text;
		private string m_action;
		[Obsolete("This is never used. From version 3 and below probably")]
		private string m_rootSrc;
		private string m_src;
		private string m_iconClass = "";
		private string m_icon;
		private string m_openIcon;
		private string m_nodeType;
		private string m_nodeID;
		private string m_treeType;

		/// <summary>
		/// Set to true when a node is created with CreateRootNode
		/// </summary>
		internal bool IsRoot { get; private set; }

		/// <summary>
		/// Generally the tree type and node type are the same but in some cased they are not so
		/// we need to store the tree type too which is read only.
		/// </summary>
		public string TreeType
		{
			get { return m_treeType; }
			internal set { m_treeType = value; }
		}

        public bool HasChildren
        {
            get
            {
                return m_HasChildren ?? !string.IsNullOrEmpty(this.Source); //defaults to true if source is specified
            }
            set
            {
                m_HasChildren = value;
            }
        }
        private bool? m_HasChildren = null;

		public string NodeID
		{
			get { return m_nodeID; }
			set { m_nodeID = value; }
		}

		/// <summary>
		/// The tree node text
		/// </summary>
		public string Text
		{
			get { return m_text; }
			set { m_text = value; }
		}

		/// <summary>
		/// The CSS class of the icon to use for the node
		/// </summary>
		public string IconClass
		{
			get { return m_iconClass; }
			set { m_iconClass = value; }
		}

		/// <summary>
		/// The JavaScript action for the node
		/// </summary>
		public string Action
		{
			get { return m_action; }
			set { m_action = value; }
		}

		/// <summary>
		/// A string of letters representing actions for the context menu
		/// </summary>
		public List<IAction> Menu
		{
			get { return m_menu; }
			set { m_menu = value; }
		}

		/// <summary>
		/// The xml source for the child nodes (a URL)
		/// </summary>      
		public string Source
		{
			get { return m_src; }
			set { m_src = value; }
		}

		/// <summary>
		/// The path to the icon to display for the node
		/// </summary>
		public string Icon
		{
			get { return m_icon; }
			set { m_icon = value; }
		}

		/// <summary>
		/// The path to the icon to display for the node if the node is showing it's children
		/// </summary>
		public string OpenIcon
		{
			get { return m_openIcon; }
			set { m_openIcon = value; }
		}

		/// <summary>
		/// Normally just the type of tree being rendered.
		/// This should really only be set with this property in very special cases
		/// where the create task for a node in the same tree as another node is different.		
		/// </summary>		
		public string NodeType
		{
			get { return m_nodeType; }
			set { m_nodeType = value; }
		}

		/// <summary>
		/// Used by the content tree and flagged as true if the node is not published
		/// </summary>
		[Obsolete("Use the XmlTreeNode.NodeStyle object to set node styles")]
		public bool? NotPublished
		{
			get { return m_notPublished; }
			set { m_notPublished = value; }
		}

		/// <summary>
		/// Used by the content tree and flagged as true if the node is protected
		/// </summary>
		[Obsolete("Use the XmlTreeNode.NodeStyle object to set node styles")]
		public bool? IsProtected
		{
			get { return m_isProtected; }
			set
			{
				m_isProtected = value;
				if (m_isProtected.HasValue && m_isProtected.Value)
					this.Style.SecureNode();
			}
		}

		/// <summary>
		/// Returns the styling object used to add common styles to a node
		/// </summary>
		public NodeStyle Style
		{
			get { return m_nodeStyle; }
		}

		/// <summary>
		/// Used to add common styles to an XmlTreeNode.
		/// This also adds the ability to add a custom class which will add the class to the li node
		/// that is rendered in the tree whereas the IconClass property of the XmlTreeNode object
		/// adds a class to the anchor of the li node.
		/// </summary>
		public sealed class NodeStyle
		{
			internal NodeStyle()
			{
				AppliedClasses = new List<string>();
			}

            private const string DimNodeCssClass = "not-published";
            private const string HighlightNodeCssClass = "has-unpublished-version";
            private const string SecureNodeCssClass = "protected";

			internal List<string> AppliedClasses { get; private set; }

			/// <summary>
			/// Dims the color of the node
			/// </summary>
			public void DimNode()
			{
				if (!AppliedClasses.Contains(DimNodeCssClass))
					AppliedClasses.Add(DimNodeCssClass);
			}

			/// <summary>
			/// Adds the star icon highlight overlay to a node
			/// </summary>
			public void HighlightNode()
			{
				if (!AppliedClasses.Contains(HighlightNodeCssClass))
					AppliedClasses.Add(HighlightNodeCssClass);
			}

			/// <summary>
			/// Adds the padlock icon overlay to a node
			/// </summary>
			public void SecureNode()
			{
                if (!AppliedClasses.Contains(SecureNodeCssClass))
                    AppliedClasses.Add(SecureNodeCssClass);
			}

			/// <summary>
			/// Adds a custom class to the li node of the tree
			/// </summary>
			/// <param name="cssClass"></param>
			public void AddCustom(string cssClass)
			{
				if (!AppliedClasses.Contains(cssClass))
					AppliedClasses.Add(cssClass);
			}
		}

		/// <summary>
		/// Dims the color of the node
		/// </summary>
		///<remarks>
		///This adds the class to the existing icon class as to not override anything.
		///</remarks>
		[Obsolete("Use XmlTreeNode.Style to style nodes. Example: myNode.Style.DimNode();")]
		public void DimNode()
		{
			this.Style.DimNode();
		}

		#region IXmlSerializable Members

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.HasAttributes)
			{
				while (reader.MoveToNextAttribute())
				{
					//try to parse the name into enum
					TreeAttributes current;
					try
					{
						current = (TreeAttributes)Enum.Parse(typeof(TreeAttributes), reader.Name, true);
					}
					catch
					{
						break;
					}
					switch (current)
					{
						case TreeAttributes.nodeID:
							this.m_nodeID = reader.Value;
							break;
						case TreeAttributes.text:
							this.m_text = reader.Value;
							break;
						case TreeAttributes.iconClass:
							this.m_iconClass = reader.Value;
							break;
						case TreeAttributes.action:
							this.m_action = reader.Value;
							break;
						case TreeAttributes.menu:
							this.m_menu = (!string.IsNullOrEmpty(reader.Value) ? umbraco.BusinessLogic.Actions.Action.FromString(reader.Value) : null);
							break;
						case TreeAttributes.rootSrc:
							this.m_rootSrc = reader.Value;
							break;
						case TreeAttributes.src:
							this.m_src = reader.Value;
							break;
						case TreeAttributes.icon:
							this.m_icon = reader.Value;
							break;
						case TreeAttributes.openIcon:
							this.m_openIcon = reader.Value;
							break;
						case TreeAttributes.nodeType:
							this.m_nodeType = reader.Value;
							break;
						case TreeAttributes.notPublished:
							if (!string.IsNullOrEmpty(reader.Value)) this.m_notPublished = bool.Parse(reader.Value);
							break;
						case TreeAttributes.isProtected:
							if (!string.IsNullOrEmpty(reader.Value)) this.m_isProtected = bool.Parse(reader.Value);
							break;
						case TreeAttributes.hasChildren:
							if (!string.IsNullOrEmpty(reader.Value)) this.HasChildren = bool.Parse(reader.Value);
							break;
					}
				}
                //need to set the hasChildren property if it is null but there is a source
                //this happens when the hasChildren attribute is not set because the developer didn't know it was there
                //only occurs for ITree obviously
                if (!this.HasChildren && !string.IsNullOrEmpty(this.m_src))
                    this.HasChildren = true;

			}

			reader.Read();
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteAttributeString(TreeAttributes.nodeID.ToString(), this.m_nodeID);
			writer.WriteAttributeString(TreeAttributes.text.ToString(), this.m_text);
			writer.WriteAttributeString(TreeAttributes.iconClass.ToString(), this.m_iconClass);
			writer.WriteAttributeString(TreeAttributes.action.ToString(), this.m_action);
			writer.WriteAttributeString(TreeAttributes.menu.ToString(), (this.m_menu != null && this.m_menu.Count > 0 ? umbraco.BusinessLogic.Actions.Action.ToString(this.m_menu) : ""));
			writer.WriteAttributeString(TreeAttributes.rootSrc.ToString(), this.m_rootSrc);
			writer.WriteAttributeString(TreeAttributes.src.ToString(), this.m_src);
			writer.WriteAttributeString(TreeAttributes.icon.ToString(), this.m_icon);
			writer.WriteAttributeString(TreeAttributes.openIcon.ToString(), this.m_openIcon);
			writer.WriteAttributeString(TreeAttributes.nodeType.ToString(), this.m_nodeType);
			writer.WriteAttributeString(TreeAttributes.hasChildren.ToString(), HasChildren.ToString().ToLower());
			if (m_notPublished.HasValue) writer.WriteAttributeString(TreeAttributes.notPublished.ToString(), this.m_notPublished.Value.ToString().ToLower());
			if (m_isProtected.HasValue) writer.WriteAttributeString(TreeAttributes.isProtected.ToString(), this.m_isProtected.Value.ToString().ToLower());
		}

		internal enum TreeAttributes
		{
			nodeID, text, iconClass, action, menu, rootSrc, src, icon, openIcon, nodeType, notPublished, isProtected, hasChildren
		}

		#endregion

	}

	/// <summary>
	/// Used to serialize an XmlTree object to JSON for supporting a JSON Tree View control.
	/// </summary>
	internal class JSONTreeConverter : JavaScriptConverter
	{
		/// <summary>
		/// Not implemented as we never need to Deserialize
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="type"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Serializes an XmlTree object with the relevant values.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
		{
			XmlTree tree = obj as XmlTree;

			Dictionary<string, object> resultSet = new Dictionary<string, object>();
			Dictionary<string, object> resultTree = new Dictionary<string, object>();

			if (tree != null)
			{
				//add a count property for the count of total nodes
				resultTree.Add("Count", tree.Count);

				List<object> nodes = new List<object>();
				foreach (XmlTreeNode node in tree.treeCollection)
					nodes.Add(node);

				resultTree.Add("Nodes", nodes);
			}

			resultSet.Add("Tree", resultTree);

			return resultSet;
		}

		public override IEnumerable<Type> SupportedTypes
		{
			get { return new Type[] { typeof(XmlTree) }; }
		}
	}

	/// <summary>
	/// Used to serialize an XmlTreeNode object to JSON for supporting a JS Tree View control.
	/// </summary>
	internal class JSONTreeNodeConverter : JavaScriptConverter
	{

		/// <summary>
		/// Not implemented as we never need to Deserialize
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="type"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Serializes an XmlTreeNode object with the relevant values.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
		{
			XmlTreeNode node = obj as XmlTreeNode;

			Dictionary<string, object> result = new Dictionary<string, object>();

			if (node != null)
			{
				//add the properties
				result.Add(XmlTreeNode.TreeAttributes.notPublished.ToString(), node.NotPublished);
				result.Add(XmlTreeNode.TreeAttributes.isProtected.ToString(), node.IsProtected);
				result.Add(XmlTreeNode.TreeAttributes.text.ToString(), node.Text);
				result.Add(XmlTreeNode.TreeAttributes.action.ToString(), node.Action);
				result.Add(XmlTreeNode.TreeAttributes.src.ToString(), node.Source);
				result.Add(XmlTreeNode.TreeAttributes.iconClass.ToString(), node.IconClass);
				result.Add(XmlTreeNode.TreeAttributes.icon.ToString(), node.Icon);
				result.Add(XmlTreeNode.TreeAttributes.openIcon.ToString(), node.OpenIcon);
				result.Add(XmlTreeNode.TreeAttributes.nodeType.ToString(), node.NodeType);
				result.Add(XmlTreeNode.TreeAttributes.nodeID.ToString(), node.NodeID);

				//Add the menu as letters.
				result.Add(XmlTreeNode.TreeAttributes.menu.ToString(), node.Menu != null && node.Menu.Count > 0 ? umbraco.BusinessLogic.Actions.Action.ToString(node.Menu) : "");

				return result;
			}

			return new Dictionary<string, object>();

		}

		public override IEnumerable<Type> SupportedTypes
		{
			get { return new Type[] { typeof(XmlTreeNode) }; }
		}
	}

	/// <summary>
	/// Used to serialize an XmlTreeNode object to JSON for supporting a JS Tree View control.
	/// </summary>
	internal class JsTreeNodeConverter : JavaScriptConverter
	{

		/// <summary>
		/// A reference path to where the icons are actually stored as compared to where the tree themes folder is
		/// </summary>
		private static string IconPath = IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/images/umbraco/";

		/// <summary>
		/// Not implemented as we never need to Deserialize
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="type"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Serializes an XmlTreeNode object with the relevant values.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
		{
			XmlTreeNode node = obj as XmlTreeNode;

			Dictionary<string, object> result = new Dictionary<string, object>();

			if (node != null)
			{
				//the data object to build the node
				Dictionary<string, object> data = new Dictionary<string, object>();

				data.Add("title", node.Text);

				

				//the attributes object fot the tree node link (a) object created
				Dictionary<string, object> dataAttributes = new Dictionary<string, object>();
				string cssClass = "";
				if (node.Icon.StartsWith(".spr"))
					cssClass = "sprTree " + node.Icon.TrimStart('.');
				else
				{
					//there is no sprite so add the noSpr class
					cssClass = "sprTree noSpr";
					data.Add("icon", IconPath + node.Icon);
				}
				dataAttributes.Add("class", cssClass + (string.IsNullOrEmpty(node.IconClass) ? "" : " " + node.IconClass));
				dataAttributes.Add("href", node.Action);

				//add a metadata dictionary object, we can store whatever we want in this!
				//in this case we're going to store permissions & the tree type & the child node source url
				//For whatever reason jsTree requires this JSON output to be quoted!?!
				//This also needs to be stored in the attributes object with the class above.
				Dictionary<string, object> metadata = new Dictionary<string, object>();
				//the menu:
				metadata.Add("menu", node.Menu == null ? "" : umbraco.BusinessLogic.Actions.Action.ToString(node.Menu));
				//the tree type:
				metadata.Add("nodeType", node.NodeType);
				//the data url for child nodes:
				metadata.Add("source", node.Source);

				//the metadata/jsTree requires this property to be in a quoted JSON syntax
				JSONSerializer jsSerializer = new JSONSerializer();
				string strMetaData = jsSerializer.Serialize(metadata).Replace("\"", "'");
                dataAttributes.Add("umb:nodedata", strMetaData);

				data.Add("attributes", dataAttributes);

				//add the data structure
				result.Add("data", data);

				//state is nothing if no children
				if ((node.HasChildren || node.IsRoot) && !string.IsNullOrEmpty(node.Source))
					result.Add("state", "closed");

				//the attributes object for the tree node (li) object created
				Dictionary<string, object> attributes = new Dictionary<string, object>();
				attributes.Add("id", node.NodeID);
				attributes.Add("class", string.Join(" ", node.Style.AppliedClasses.ToArray()));
				
				if (node.IsRoot)
					attributes.Add("rel", "rootNode");
				else
					attributes.Add("rel", "dataNode");

				//the tree type should always be set, however if some developers have serialized their tree into an XmlTree
				//then there is no gaurantees that this will be set if they didn't use the create methods of XmlTreeNode.
				//So, we'll set the treetype to the nodetype if it is null, this means that the tree on the front end may
				//not function as expected when reloding a node.
				attributes.Add("umb:type", string.IsNullOrEmpty(node.TreeType) ? node.NodeType : node.TreeType);

				result.Add("attributes", attributes);

				return result;
			}

			return null;

		}

		public override IEnumerable<Type> SupportedTypes
		{
			get { return new Type[] { typeof(XmlTreeNode) }; }
		}
	}
}
