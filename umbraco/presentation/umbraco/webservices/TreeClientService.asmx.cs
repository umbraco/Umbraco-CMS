using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using umbraco.presentation.umbraco.controls;
using umbraco.cms.presentation.Trees;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.EnterpriseServices;
using System.IO;
using System.Web.UI;
using umbraco.controls.Tree;

namespace umbraco.presentation.webservices
{
	/// <summary>
	/// Client side ajax utlities for the tree
	/// </summary>
	[ScriptService]
	[WebService]
	public class TreeClientService : WebService
	{

		/// <summary>
		/// Returns a key/value object with: json, app, js as the keys
		/// </summary>	
		/// <returns></returns>
		[WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public Dictionary<string, string> GetInitAppTreeData(string app, string treeType, bool showContextMenu, bool isDialog, TreeDialogModes dialogMode, string functionToCall, string nodeKey)
		{
			Authorize();

			TreeControl treeCtl = new TreeControl()
			{
                ShowContextMenu = showContextMenu,
                IsDialog = isDialog,
                DialogMode = dialogMode,
                App = app,
                TreeType = string.IsNullOrEmpty(treeType) ? "" : treeType, //don't set the tree type unless explicitly set
                NodeKey = string.IsNullOrEmpty(nodeKey) ? "" : nodeKey,
                StartNodeID = -1, //TODO: set this based on parameters!
                FunctionToCall = string.IsNullOrEmpty(functionToCall) ? "" : functionToCall
			};

			Dictionary<string, string> returnVal = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(treeType))
            {
                //if there's not tree type specified, then render out the tree as per normal with the normal 
                //way of doing things
                returnVal.Add("json", treeCtl.GetJSONInitNode());
            }
            else
            {
               
                //get the tree that we need to render
                var tree = TreeDefinitionCollection.Instance.FindTree(treeType).CreateInstance();
                tree.ShowContextMenu = showContextMenu;
                tree.IsDialog = isDialog;
                tree.DialogMode = dialogMode;
                tree.NodeKey = string.IsNullOrEmpty(nodeKey) ? "" : nodeKey;
                tree.FunctionToCall = string.IsNullOrEmpty(functionToCall) ? "" : functionToCall;
                //this would be nice to set, but no parameters :( 
                //tree.StartNodeID =

                //now render it's start node
                XmlTree xTree = new XmlTree();
                xTree.Add(tree.RootNode);
                returnVal.Add("json", xTree.ToString());
            }

            returnVal.Add("app", app);
			returnVal.Add("js", treeCtl.JSCurrApp);

			return returnVal;
		}	

		public static void Authorize()
		{
			if (!BasePages.BasePage.ValidateUserContextID(BasePages.BasePage.umbracoUserContextID))
				throw new Exception("Client authorization failed. User is not logged in");

		}

	}
}
