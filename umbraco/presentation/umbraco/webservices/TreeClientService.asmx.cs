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
				TreeType = "",
				NodeKey = "",
				StartNodeID = -1,
				FunctionToCall = null
			};

			Dictionary<string, string> returnVal = new Dictionary<string, string>();
			returnVal.Add("json", treeCtl.GetJSONInitNode());
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
