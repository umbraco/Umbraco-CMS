using System.Xml.Serialization;
using System.Collections;
using System;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Umbraco.Core.IO;
using Umbraco.Web.UI.Pages;
using Umbraco.Web._Legacy.Actions;
using Action = Umbraco.Web._Legacy.Actions.Action;

namespace umbraco.cms.presentation.Trees
{
	/// <summary>
	/// Used for serializing data to XML as the data structure for the JavaScript tree
	/// </summary>
	[XmlRoot(ElementName = "tree", IsNullable = false), Serializable]
	public class XmlTree
	{

		public XmlTree()
		{
			Init();
		}
        
		private void Init()
		{
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
			
            // The apps dashboard action will be used
            xNode.Action = "javascript:" + ClientTools.Scripts.OpenDashboard(bTree.app);
			
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
							this.m_menu = (!string.IsNullOrEmpty(reader.Value) ? Umbraco.Web._Legacy.Actions.Action.FromString(reader.Value) : null);
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
			writer.WriteAttributeString(TreeAttributes.menu.ToString(), (this.m_menu != null && this.m_menu.Count > 0 ? Umbraco.Web._Legacy.Actions.Action.ToString(this.m_menu) : ""));
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

}
