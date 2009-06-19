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
		/// <param name="app"></param>
		/// <param name="showContextMenu"></param>
		/// <param name="isDialog"></param>
		/// <returns></returns>
		[WebMethod]
		public Dictionary<string, string> GetInitAppTreeData(string app, bool showContextMenu, bool isDialog)
		{
			Authorize();

			TreeControl treeCtl = new TreeControl();
			TreeService treeSvc = new TreeService();
			treeSvc.App = app;
			treeSvc.ShowContextMenu = showContextMenu;
			treeSvc.IsDialog = isDialog;

			treeCtl.SetTreeService(treeSvc);

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
