using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.contentitem;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.DataLayer;
using System.Collections.Specialized;
using umbraco.IO;

namespace umbraco.cms.presentation.Trees
{

	/// <summary>
	/// All Trees rely on the properties of an ITreeService interface. This has been created to avoid having trees
	/// dependant on the HttpContext
	/// </summary>
	public interface ITreeService
	{
		/// <summary>
		/// The NodeKey is a string representation of the nodeID. Generally this is used for tree's whos node's unique key value is a string in instead 
		/// of an integer such as folder names.
		/// </summary>
		string NodeKey { get; }
		int StartNodeID { get; }
		bool ShowContextMenu { get; }
		bool IsDialog { get; }
		TreeDialogModes DialogMode { get; }
		string FunctionToCall { get; }
	}

	/// <summary>
	/// A utility class to aid in creating the URL for returning XML for a tree structure and
	/// for reading the parameters from the URL when a request is made.
	/// </summary>
	public class TreeService : ITreeService
	{

		/// <summary>
		/// Default empty constructor
		/// </summary>
		public TreeService() { }

		/// <summary>
		/// Constructor to assign all TreeService properties except nodeKey in one call
		/// </summary>
		/// <param name="startNodeID"></param>
		/// <param name="treeType"></param>
		/// <param name="showContextMenu"></param>
		/// <param name="isDialog"></param>
		/// <param name="dialogMode"></param>
		/// <param name="app"></param>
		public TreeService(int? startNodeID, string treeType, bool? showContextMenu,
			bool? isDialog, TreeDialogModes dialogMode, string app)
		{
			m_startNodeID = startNodeID;
			m_treeType = treeType;
			m_showContextMenu = showContextMenu;
			m_isDialog = isDialog;
			m_dialogMode = dialogMode;
			m_app = app;
		}

		/// <summary>
		/// Constructor to assign all TreeService properties in one call
		/// </summary>
		/// <param name="startNodeID"></param>
		/// <param name="treeType"></param>
		/// <param name="showContextMenu"></param>
		/// <param name="isDialog"></param>
		/// <param name="dialogMode"></param>
		/// <param name="app"></param>
		/// <param name="nodeKey"></param>
		public TreeService(int? startNodeID, string treeType, bool? showContextMenu,
			bool? isDialog, TreeDialogModes dialogMode, string app, string nodeKey)
		{
			m_startNodeID = startNodeID;
			m_treeType = treeType;
			m_showContextMenu = showContextMenu;
			m_isDialog = isDialog;
			m_dialogMode = dialogMode;
			m_app = app;
			m_nodeKey = nodeKey;
		}

		public TreeService(int? startNodeID, string treeType, bool? showContextMenu,
			bool? isDialog, TreeDialogModes dialogMode, string app, string nodeKey, string functionToCall)
		{
			m_startNodeID = startNodeID;
			m_treeType = treeType;
			m_showContextMenu = showContextMenu;
			m_isDialog = isDialog;
			m_dialogMode = dialogMode;
			m_app = app;
			m_nodeKey = nodeKey;
			m_functionToCall = functionToCall;
		}

		public const string TREE_URL = "tree.aspx";
		public const string INIT_URL = "treeinit.aspx";
		public const string PICKER_URL = "treepicker.aspx";

		private int? m_startNodeID;
		private string m_treeType;
		private bool? m_showContextMenu;
		private bool? m_isDialog;
		private TreeDialogModes m_dialogMode;
		private string m_app;
		private string m_nodeKey;
		private string m_functionToCall;

		#region Public Properties

		public string FunctionToCall
		{
			get { return m_functionToCall; }
			set { m_functionToCall = value; }
		}

		public string NodeKey
		{
			get { return m_nodeKey; }
			set { m_nodeKey = value; }
		}

		public int StartNodeID
		{
			get { return m_startNodeID ?? -1; }
			set { m_startNodeID = value; }
		}

		public string TreeType
		{
			get { return m_treeType; }
			set { m_treeType = value; }
		}

		public bool ShowContextMenu
		{
			get { return m_showContextMenu ?? true; }
			set { m_showContextMenu = value; }
		}

		public bool IsDialog
		{
			get { return m_isDialog ?? false; }
			set { m_isDialog = value; }
		}

		public TreeDialogModes DialogMode
		{
			get { return m_dialogMode; }
			set { m_dialogMode = value; }
		}

		public string App
		{
			get { return m_app; }
			set { m_app = value; }
		}
		#endregion

		/// <summary>
		/// Returns the url for servicing the xml tree request based on the parameters specified on this class.
		/// </summary>
		/// <returns>Tree service url as a string</returns>
		public string GetServiceUrl()
		{
			return SystemDirectories.Umbraco + "/" + GetUrl(TREE_URL);
		}

		/// <summary>
		/// Static method to return the tree service url with the specified parameters
		/// </summary>
		/// <param name="startNodeID"></param>
		/// <param name="treeType"></param>
		/// <param name="showContextMenu"></param>
		/// <param name="isDialog"></param>
		/// <param name="dialogMode"></param>
		/// <param name="app"></param>
		/// <param name="nodeKey"></param>
		/// <param name="functionToCall"></param>
		/// <returns></returns>
		public static string GetServiceUrl(int? startNodeID, string treeType, bool? showContextMenu,
			bool? isDialog, TreeDialogModes dialogMode, string app, string nodeKey, string functionToCall)
		{
			TreeService treeSvc = new TreeService(startNodeID, treeType, showContextMenu, isDialog, dialogMode, app, nodeKey, functionToCall);
			return treeSvc.GetServiceUrl();
		}

		/// <summary>
		/// Returns the url for initializing the tree based on the parameters specified on this class
		/// </summary>
		/// <returns></returns>
		public string GetInitUrl()
		{
			return IOHelper.ResolveUrl( SystemDirectories.Umbraco + "/" + GetUrl(INIT_URL) );
		}

		/// <summary>
		/// static method to return the tree init url with the specified parameters
		/// </summary>
		/// <param name="startNodeID"></param>
		/// <param name="treeType"></param>
		/// <param name="showContextMenu"></param>
		/// <param name="isDialog"></param>
		/// <param name="dialogMode"></param>
		/// <param name="app"></param>
		/// <param name="nodeKey"></param>
		/// <param name="functionToCall"></param>
		/// <returns></returns>
		public static string GetInitUrl(int? startNodeID, string treeType, bool? showContextMenu,
		   bool? isDialog, TreeDialogModes dialogMode, string app, string nodeKey, string functionToCall)
		{
			TreeService treeSvc = new TreeService(startNodeID, treeType, showContextMenu, isDialog, dialogMode, app, nodeKey, functionToCall);
			return treeSvc.GetInitUrl();
		}

		/// <summary>
		/// Returns the url for the tree picker (used on modal windows) based on the parameters specified on this class
		/// </summary>
		/// <param name="useSubModal"></param>
		/// <returns></returns>
		public static string GetPickerUrl(bool useSubModal, string app, string treeType)
		{
			TreeService treeSvc = new TreeService();
			treeSvc.App = app;
			treeSvc.TreeType = treeType;
			return treeSvc.GetPickerUrl(useSubModal);
		}

		/// <summary>
		/// Returns the url for the tree picker (used on modal windows) based on the parameters specified on this class
		/// </summary>
		/// <param name="useSubModal"></param>
		/// <returns></returns>
		public string GetPickerUrl(bool useSubModal)
		{
			string url = IOHelper.ResolveUrl( SystemDirectories.Umbraco + "/dialogs/" + GetUrl(PICKER_URL) );
			return url + (useSubModal ? "&useSubModal=true" : "");
		}

		/// <summary>
		/// Generates the URL parameters for the tree service.
		/// </summary>
		/// <param name="pageUrl">the base url (i.e. tree.aspx)</param>
		/// <returns></returns>
		private string GetUrl(string pageUrl)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(pageUrl);
			//insert random
			sb.Append(string.Format("?rnd={0}", Guid.NewGuid()));

			sb.Append(string.Format("&id={0}", this.StartNodeID.ToString()));
			if (!string.IsNullOrEmpty(this.TreeType)) sb.Append(string.Format("&treeType={0}", this.TreeType));
			if (!string.IsNullOrEmpty(this.NodeKey)) sb.Append(string.Format("&nodeKey={0}", this.NodeKey));
			sb.Append(string.Format("&contextMenu={0}", this.ShowContextMenu.ToString().ToLower()));
			sb.Append(string.Format("&isDialog={0}", this.IsDialog.ToString().ToLower()));
			if (this.DialogMode != TreeDialogModes.none) sb.Append(string.Format("&dialogMode={0}", this.DialogMode.ToString()));
			if (!string.IsNullOrEmpty(this.App)) sb.Append(string.Format("&app={0}", this.App));

			return sb.ToString();
		}

	}

	/// <summary>
	/// An ITreeService class that returns the values found in the Query String or any dictionary
	/// </summary>
	internal class TreeRequestParams : ITreeService
	{
		private TreeRequestParams() { }

		private Dictionary<string, string> m_params;

		public static TreeRequestParams FromQueryStrings()
		{
			Dictionary<string, string> p = new Dictionary<string, string>();
			foreach (string key in HttpContext.Current.Request.QueryString.Keys)
			{
				p.Add(key, HttpContext.Current.Request.QueryString[key]);
				//p.Add(item.Key.ToString(), item.Value.ToString());
			}
			return FromDictionary(p);
		}

		public static TreeRequestParams FromDictionary(Dictionary<string, string> items)
		{
			TreeRequestParams treeParams = new TreeRequestParams();
			treeParams.m_params = items;
			return treeParams;
		}

		public string NodeKey
		{
			get
			{
				return (m_params.ContainsKey("nodeKey") ? m_params["nodeKey"] : ""); 				
			}
		}
		public string Application
		{
			get
			{
				return (m_params.ContainsKey("app") ? m_params["app"] : m_params.ContainsKey("appAlias") ? m_params["appAlias"] : ""); 	
			}
		}
		public int StartNodeID
		{
			get
			{
				string val = (m_params.ContainsKey("id") ? m_params["id"] : ""); 	
				int sNodeID;
				if (int.TryParse(HttpContext.Current.Request.QueryString["id"], out sNodeID))
					return sNodeID;
				return -1;
			}
		}
		public string FunctionToCall
		{
			get
			{
				return (m_params.ContainsKey("functionToCall") ? m_params["functionToCall"] : ""); 	
			}
		}
		public bool IsDialog
		{
			get
			{
				bool value;
				if (m_params.ContainsKey("isDialog"))
					if (bool.TryParse(m_params["isDialog"], out value))
						return value;
				return false;
			}
		}
		public TreeDialogModes DialogMode
		{
			get
			{
				if (m_params.ContainsKey("dialogMode") && IsDialog)
				{
					try
					{
						return (TreeDialogModes)Enum.Parse(typeof(TreeDialogModes), m_params["dialogMode"]);
					}
					catch
					{
						return TreeDialogModes.none;
					}
				}
				return TreeDialogModes.none;
			}
		}
		public bool ShowContextMenu
		{
			get
			{
				bool value;
				if (m_params.ContainsKey("contextMenu"))
					if (bool.TryParse(m_params["contextMenu"], out value))
						return value;
				return true;
			}
		}
		public string TreeType
		{
			get
			{
				return (m_params.ContainsKey("treeType") ? m_params["treeType"] : ""); 	
			}
		}
	}
}
