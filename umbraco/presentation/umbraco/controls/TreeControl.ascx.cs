using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using umbraco.interfaces;
using System.Text.RegularExpressions;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic.Utils;
using System.Text;
using umbraco.cms.presentation.Trees;
using umbraco.BasePages;
using System.Web.Services;

namespace umbraco.presentation.umbraco.controls
{

	/// <summary>
	/// The Umbraco tree control.
	/// <remarks>If this control doesn't exist on an UmbracoEnsuredPage it will not work.</remarks>
	/// </summary>
	public partial class TreeControl : System.Web.UI.UserControl
	{

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			EnableViewState = false;
		}

		public enum TreeMode
		{
			Standard, Checkbox, InheritedCheckBox
		}

		/// <summary>
		/// If there is not application or tree specified in a query string then this is the application to load.
		/// </summary>
		private const string DEFAULT_APP = "content";

		private List<BaseTree> m_ActiveTrees = new List<BaseTree>();
		private List<BaseTree> m_AllAppTrees = new List<BaseTree>();
		private List<TreeDefinition> m_ActiveTreeDefs = null;
		private TreeMode m_TreeType = TreeMode.Standard;
		private bool m_IsInit = false;
		private TreeService m_TreeService = null;

		/// <summary>
		/// returns the current tree service being used to render the tree
		/// </summary>
		public TreeService TreeSvc
		{
			get
			{
				return m_TreeService;
			}			
		}

		/// <summary>
		/// Can be set explicitly which will override what is in query strings. 
		/// Useful for rendering out a tree dynamically.
		/// </summary>
		public void SetTreeService(TreeService srv)
		{
			m_TreeService = srv;
			Initialize();
		}

		/// <summary>
		/// Allows for checkboxes to be used with the tree. Default is standard.
		/// </summary>
		public TreeMode TreeType
		{
			get
			{
				return m_TreeType;
			}
			set
			{
				m_TreeType = value;
			}
		}

		/// <summary>
		/// Used to create a different container id for the tree control. This is done so that
		/// there is a way to differentiate between multiple trees if required. This is currently used
		/// to distinguish which tree is the main umbraco tree as opposed to dialog trees.
		/// </summary>
		public string CustomContainerId { get; set; }
		
		protected void Initialize()
		{
			//use the query strings if the TreeParams isn't explicitly set
			if (m_TreeService == null)
			{
				TreeRequestParams treeParams = TreeRequestParams.FromQueryStrings();
				m_TreeService = new TreeService()
				{
					ShowContextMenu = treeParams.ShowContextMenu,
					IsDialog = treeParams.IsDialog,
					DialogMode = treeParams.DialogMode,
					App = treeParams.Application,
					TreeType = treeParams.TreeType
				};
			}

			//ensure that the application is setup correctly
			m_TreeService.App = CurrentApp;	

			// Validate permissions
			if (!BasePages.BasePage.ValidateUserContextID(BasePages.BasePage.umbracoUserContextID))
				return;
			UmbracoEnsuredPage page = new UmbracoEnsuredPage();
			if (!page.ValidateUserApp(TreeSvc.App))
				throw new ArgumentException("The current user doesn't have access to this application. Please contact the system administrator.");

			//find all tree definitions that have the current application alias that are ACTIVE
			m_ActiveTreeDefs = TreeDefinitionCollection.Instance.FindActiveTrees(TreeSvc.App);
			//find all tree defs that exists for the current application regardless of if they are active
			List<TreeDefinition> appTreeDefs = TreeDefinitionCollection.Instance.FindTrees(TreeSvc.App);

			//Create the BaseTree's based on the tree definitions found
			foreach (TreeDefinition treeDef in appTreeDefs)
			{
				//create the tree and initialize it
				BaseTree bTree = treeDef.CreateInstance();
				bTree.SetTreeParameters(TreeSvc);

				//store the created tree
				m_AllAppTrees.Add(bTree);
				if (treeDef.Tree.Initialize)
					m_ActiveTrees.Add(bTree);
			}

			m_IsInit = true;
		}

		/// <summary>
		/// This calls the databind method to bind the data binding syntax on the front-end.
		/// <remarks>
		/// Databinding was used instead of inline tags in case the tree properties needed to be set
		/// by other classes at runtime
		/// </remarks>
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if (!m_IsInit)
				Initialize();

			//Render out the JavaScript associated with all of the trees for the application
			RenderTreeJS();

			DataBind();
		}

		/// <summary>
		/// Returns the JSON markup for the full context menu
		/// </summary>
		public string GetJSONContextMenu()
		{
			JTreeContextMenu menu = new JTreeContextMenu();
			return menu.RenderJSONMenu();
		}

		/// <summary>
		/// Returns the JSON markup for one node
		/// </summary>
		/// <param name="treeAlias"></param>
		/// <param name="nodeId"></param>
		/// <returns></returns>
		public string GetJSONNode(string nodeId)
		{
			if (!m_IsInit)
				Initialize();

			if (string.IsNullOrEmpty(TreeSvc.TreeType))
			{
				throw new ArgumentException("The TreeType is not set on the tree service");
			}

			BaseTree tree = m_ActiveTrees.Find(
				delegate(BaseTree t)
				{
					return (t.TreeAlias == TreeSvc.TreeType);
				}
			);
			return tree.GetSerializedNodeData(nodeId);
		}

		/// <summary>
		/// Returns the JSON markup for the first node in the tree
		/// </summary>
		public string GetJSONInitNode()
		{
			if (!m_IsInit)
				Initialize();

			//if there is only one tree to render, we don't want to have a node to hold sub trees, we just want the
			//stand alone tree, so we'll just add a TreeType to the TreeService and ensure that the right method gets loaded in tree.aspx
			if (m_ActiveTrees.Count == 1)
			{
				TreeSvc.TreeType = m_ActiveTreeDefs[0].Tree.Alias;

				//convert the menu to a string
				//string initActions = (TreeSvc.ShowContextMenu ? Action.ToString(m_ActiveTrees[0].RootNodeActions) : "");

				//Since there's only 1 tree, render out the tree's RootNode properties
				XmlTree xTree = new XmlTree();
				xTree.Add(m_ActiveTrees[0].RootNode);
				return xTree.ToString();
			}
			else
			{

				//If there is more than 1 tree for the application than render out a 
				//container node labelled with the current application.
				XmlTree xTree = new XmlTree();
				XmlTreeNode xNode = XmlTreeNode.CreateRoot(new NullTree(CurrentApp));
				xNode.Text = ui.Text("sections", TreeSvc.App, UmbracoEnsuredPage.CurrentUser);
				xNode.Source = TreeSvc.GetServiceUrl();
				xNode.Action = ClientTools.Scripts.OpenDashboard(TreeSvc.App);
				xNode.NodeType = TreeSvc.App.ToLower();
				xNode.NodeID = "-1";
				xNode.Icon = ".sprTreeFolder";
				xTree.Add(xNode);
				return xTree.ToString();
			}
		}

		/// <summary>
		/// Returns the requires JavaScript as a string for the current application
		/// </summary>
		public string JSCurrApp
		{
			get
			{
				StringBuilder javascript = new StringBuilder();
				foreach (BaseTree bTree in m_AllAppTrees)
					bTree.RenderJS(ref javascript);
				return javascript.ToString();
			}
		}

		/// <summary>
		/// Return the current application alias. If neither the TreeType of Application is specified
		/// than return the default application. If the Application is null but there is a TreeType then
		/// find the application that the tree type is associated with.
		/// </summary>
		protected string CurrentApp
		{
			get
			{			
				//if theres an treetype specified but no application
				if (string.IsNullOrEmpty(TreeSvc.App) &&
					!string.IsNullOrEmpty(TreeSvc.TreeType))
				{
					TreeDefinition treeDef = TreeDefinitionCollection.Instance.FindTree(TreeSvc.TreeType);
					if (treeDef != null)
						return treeDef.App.alias;
				}
				else if (!string.IsNullOrEmpty(TreeSvc.App))
					return TreeSvc.App;

				//if everything is null then return the default app
				return DEFAULT_APP;
			}
		}

		private void RenderTreeJS()
		{
			Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "Trees", JSCurrApp, true);
		}


	}

	internal class JTreeContextMenu
	{
		public string RenderJSONMenu()
		{

			JSONSerializer jSSerializer = new JSONSerializer();

			jSSerializer.RegisterConverters(new List<JavaScriptConverter>() 
			{ 	
				new JTreeContextMenuItem()
			});

			List<IAction> allActions = new List<IAction>();
			foreach (IAction a in global::umbraco.BusinessLogic.Actions.Action.GetAll())
			{
				if (!string.IsNullOrEmpty(a.Alias) && (!string.IsNullOrEmpty(a.JsFunctionName) || !string.IsNullOrEmpty(a.JsSource)))
				{
					allActions.Add(a);
				}

			}


			return jSSerializer.Serialize(allActions);
		}
	}

	internal class JTreeContextMenuItem : JavaScriptConverter
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

		public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
		{
			//{
			//    "id": "create",
			//    "label": "Create",
			//    "icon": "create.png",
			//    "visible": function(NODE, TREE_OBJ) { if (NODE.length != 1) return false; return TREE_OBJ.check("creatable", NODE); },
			//    "action": function(NODE, TREE_OBJ) { TREE_OBJ.create(false, NODE); }
			//}


			IAction a = (IAction)obj;
			Dictionary<string, object> data = new Dictionary<string, object>();

			data.Add("id", a.Letter);
			data.Add("label", ui.Text(a.Alias));

			if (a.Icon.StartsWith("."))
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(string.Format("<div class='menuSpr {0}'>", a.Icon.TrimStart('.')));
				sb.Append("</div>");
				sb.Append("<div class='menuLabel'>");
				sb.Append(data["label"].ToString());
				sb.Append("</div>");
				data["label"] = sb.ToString();
			}
			else
			{
				data.Add("icon", a.Icon);
			}

			//required by jsTree
			data.Add("visible", JSONSerializer.ToJSONObject("function() {return true;}"));
			//The action handler is what is assigned to the IAction, but for flexibility, we'll call our onContextMenuSelect method which will need to return true if the function is to execute.
			//TODO: Check if there is a JSSource
			data.Add("action", JSONSerializer.ToJSONObject("function(N,T){" + a.JsFunctionName + ";}"));

			return data;

		}

		/// <summary>
		/// TODO: Find out why we can't just return IAction as one type (JavaScriptSerializer doesn't seem to pick up on it)
		/// </summary>
		public override IEnumerable<Type> SupportedTypes
		{
			get
			{
				List<Type> types = new List<Type>();
				foreach (IAction a in global::umbraco.BusinessLogic.Actions.Action.GetAll())
				{
					types.Add(a.GetType());
				}
				return types;
			}


		}
	}
}