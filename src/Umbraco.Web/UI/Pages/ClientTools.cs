using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Umbraco.Core.IO;
using umbraco.BasePages;
using System.Web.UI;
using umbraco.BusinessLogic;
using Umbraco.Core;

namespace Umbraco.Web.UI.Pages
{

	/// <summary>
	/// Renders the client side code necessary to interact with the Umbraco client side API.
	/// Each method returns an instance of this class so you can chain calls together.
	/// </summary>
	public sealed class ClientTools
	{

		public ClientTools(Page page)
		{
			_page = page;
		}

		/// <summary>
		/// Returns the string markup for the JavaScript that is rendered.
		/// If referencing JavaScript scripts in the backend, this class should be used
		/// in case future changes to the client code is change, this will remain intact.
		/// </summary>
		public static class Scripts
		{
			internal const string ClientMgrScript = "UmbClientMgr";
			public static string GetAppActions { get { return string.Format("{0}.appActions()", ClientMgrScript); } }
			public static string GetMainWindow { get { return string.Format("{0}.mainWindow()", ClientMgrScript); } }
			public static string GetMainTree { get { return string.Format("{0}.mainTree()", ClientMgrScript); } }
			public static string GetContentFrame() { return string.Format("{0}.contentFrame()", ClientMgrScript); }
			public static string ShiftApp(string appAlias)
			{
                return string.Format(ClientMgrScript + ".historyManager().addHistory('{0}')", appAlias);
			}
			public static string OpenDashboard(string app)
			{
				return string.Format(GetAppActions + ".openDashboard('{0}');", app);
			}
			public static string RefreshAdmin { get { return "setTimeout('" + GetMainWindow + ".location.reload()', {0});"; } }
			public static string ShowSpeechBubble { get { return GetMainWindow + ".UmbSpeechBubble.ShowMessage('{0}','{1}', '{2}');"; } }
			public static string ChangeContentFrameUrl(string url) {
				return string.Format(ClientMgrScript + ".contentFrame('{0}');", url);
			}
            public static string ReloadContentFrameUrlIfPathLoaded(string url)
            {
                return string.Format(ClientMgrScript + ".reloadContentFrameUrlIfPathLoaded('{0}');", url);
            }
            public static string ReloadLocation { get { return string.Format(ClientMgrScript + ".reloadLocation();"); } }
            public static string ChildNodeCreated = GetMainTree + ".childNodeCreated();";
			public static string SyncTree { get { return GetMainTree + ".syncTree('{0}', {1});"; } }
			public static string ClearTreeCache { get { return GetMainTree + ".clearTreeCache();"; } }
			public static string CopyNode { get { return GetMainTree + ".copyNode('{0}', '{1}');"; } }
			public static string MoveNode { get { return GetMainTree + ".moveNode('{0}', '{1}');"; } }
			public static string ReloadActionNode { get { return GetMainTree + ".reloadActionNode({0}, {1}, null);"; } }
			public static string SetActiveTreeType { get { return GetMainTree + ".setActiveTreeType('{0}');"; } }
            public static string RefreshTree { get { return GetMainTree + ".refreshTree();"; } }
            public static string RefreshTreeType { get { return GetMainTree + ".refreshTree('{0}');"; } }
            public static string CloseModalWindow()
            {
                return string.Format("{0}.closeModalWindow();", ClientMgrScript);
            }
            public static string CloseModalWindow(string rVal)
            {
                return string.Format("{0}.closeModalWindow('{1}');", ClientMgrScript, rVal);
            }
            public static string OpenModalWindow(string url, string name, int width, int height)
            {
                return OpenModalWindow(url, name, true, width, height, 0, 0, "", "");
            }
            public static string OpenModalWindow(string url, string name, bool showHeader, int width, int height, int top, int leftOffset, string closeTriggers, string onCloseCallback)
            {
                return string.Format("{0}.openModalWindow('{1}', '{2}', {3}, {4}, {5}, {6}, {7}, '{8}', '{9}');",
                    new object[] { ClientMgrScript, url, name, showHeader.ToString().ToLower(), width, height, top, leftOffset, closeTriggers, onCloseCallback });
            }
		}

		private readonly Page _page;

		/// <summary>
		/// This removes all tree JSON data cached in the client browser.
		/// Useful when you want to ensure that the tree is reloaded from live data.
		/// </summary>
		/// <returns></returns>
		public ClientTools ClearClientTreeCache()
		{
			RegisterClientScript(Scripts.ClearTreeCache);
			return this;
		}

		/// <summary>
		/// Change applications
		/// </summary>
		/// <returns></returns>
		public ClientTools ShiftApp(string appAlias)
		{
			RegisterClientScript(Scripts.ShiftApp(appAlias));
			return this;
		}
		
		/// <summary>
		/// Refresh the entire administration console after a specified amount of time.
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public ClientTools RefreshAdmin(int seconds)
		{
			RegisterClientScript(string.Format(Scripts.RefreshAdmin, seconds * 1000));
			return this;
		}

        /// <summary>
        /// Refreshes the entire current tree
        /// </summary>
        /// <returns></returns>
        public ClientTools RefreshTree()
        {
            RegisterClientScript(Scripts.RefreshTree);
            return this;
        }

        public ClientTools RefreshTree(string treeType)
        {
            RegisterClientScript(string.Format(Scripts.RefreshTreeType, treeType));
            return this;
        }
		
		/// <summary>
		/// A reference to the umbraco UI component "speechbubble". The speechbubble appears in the lower right corner of the screen, notifying users of events
		/// </summary>
		/// <param name="i">The speechbubble icon.</param>
		/// <param name="header">The speechbubble header.</param>
		/// <param name="body">The body text</param>
		public ClientTools ShowSpeechBubble(SpeechBubbleIcon i, string header, string body)
		{
			RegisterClientScript(string.Format(Scripts.ShowSpeechBubble, i.ToString(), header.Replace("'", "\\'"), body.Replace("'", "\\'")));
			return this;
		}
		
		/// <summary>
		/// Changes the content in the content frame to the specified URL
		/// </summary>
		/// <param name="url"></param>
		public ClientTools ChangeContentFrameUrl(string url)
		{
            //don't load if there is no url
			if (string.IsNullOrEmpty(url)) return this;

            url = EnsureUmbracoUrl(url);

            RegisterClientScript(Scripts.ChangeContentFrameUrl(url));
			
            return this;
		}

        /// <summary>
        /// Reloads the content in the content frame if the specified URL is currently loaded
        /// </summary>
        /// <param name="url"></param>
        public ClientTools ReloadContentFrameUrlIfPathLoaded(string url)
        {
            if (string.IsNullOrEmpty(url)) return this;

            url = EnsureUmbracoUrl(url);

            RegisterClientScript(Scripts.ReloadContentFrameUrlIfPathLoaded(url));

            return this;
        }

        /// <summary>
        /// Reloads location, refreshing what is in the content frame
        /// </summary>
        public ClientTools ReloadLocation()
        {
            RegisterClientScript(Scripts.ReloadLocation);

            return this;
        }

        private string EnsureUmbracoUrl(string url)
        {
            if (url.StartsWith("/") && url.StartsWith(IOHelper.ResolveUrl(SystemDirectories.Umbraco)) == false)
            {
                url = IOHelper.ResolveUrl(SystemDirectories.Umbraco).EnsureEndsWith('/') + url;
            }

            if (url.Trim().StartsWith("~"))
                url = IOHelper.ResolveUrl(url);

            return url;
        }

        /// <summary>
        /// Shows the dashboard for the given application
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public ClientTools ShowDashboard(string app)
		{
            return ChangeContentFrameUrl(SystemDirectories.Umbraco + string.Format("/dashboard.aspx?app={0}", app));
		}
		
		/// <summary>
		/// Reloads the children of the current action node and selects the node that didn't exist there before.
		/// If the client side system cannot determine which node is new, then no node is selected.		
		/// </summary>
		/// <remarks>
		/// This is used by many create dialogs, however the sync method should be used based on the full path of the
		/// node but because the current Umbraco implementation of ITask only returns a url to load, there's no way
		/// to determine what the full path of the new child is.
		/// </remarks>
		/// <returns></returns>
		public ClientTools ChildNodeCreated()
		{
			RegisterClientScript(Scripts.ChildNodeCreated);
			return this;
		}		

		/// <summary>
		/// Synchronizes the tree to the path specified.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="forceReload">
		/// If set to true, will ensure that the node to be synced has it's data 
		/// reloaded from the server. Otherwise, if the node already exists, the tree will simply sync to the node
		/// that is already there.
		/// </param>
		/// <remarks>
		/// This will work for any tree, however you would need to know the path of the node. Currently, media and content
		/// are the only trees that store a path, however, if you were working in the template tree for example, a path to a
		/// node could be "init,1090" and this method would still work.
		/// 
		/// Sync tree will works by syncing the active tree type. This can be specified explicitly by calling SetActiveTreeType. 
		/// This will allow developers to sync many trees in one application at one time if needed.
		/// </remarks>
		/// <example>
		/// <![CDATA[
		/// //if you had both the media and content trees in the same app, you could sync both at the same
		/// //time by doing:
		/// BasePage.Current.ClientTools
		///		.SetActiveTreeType("content")
		///			.SyncTree("-1,100,200")
		///		.SetActiveTreeType("media")
		///			.SyncTree("-1,323,355");
		/// ]]>
		/// </example>
		public ClientTools SyncTree(string path, bool forceReload)
		{
			RegisterClientScript(string.Format(Scripts.SyncTree, path, forceReload.ToString().ToLower()));
			return this;
		}

		public ClientTools CopyNode(string currNodeId, string newParentPath)
		{
			RegisterClientScript(string.Format(Scripts.CopyNode, currNodeId, newParentPath));
			return this;
		}

		public ClientTools MoveNode(string currNodeId, string newParentPath)
		{
			RegisterClientScript(string.Format(Scripts.MoveNode, currNodeId, newParentPath));
			return this;
		}

		/// <summary>
		/// Reloads only the last node that the user interacted with via the context menu. To reload a specify node, use SyncTree.
		/// </summary>
		/// <param name="reselect"></param>
		/// <param name="reloadChildren"></param>
		/// <remarks>
		/// If for whatever reason the client side system cannot just refresh the one node, the system will use jsTree's built in 
		/// refresh tool, this however won't allow for reselect or reloadChildren. Most trees will work with the single node
		/// refresh but 3rd party tools may have poorly built tree data models.
		/// </remarks>
		public ClientTools ReloadActionNode(bool reselect, bool reloadChildren)
		{
			RegisterClientScript(string.Format(Scripts.ReloadActionNode, (!reselect).ToString().ToLower(), (!reloadChildren).ToString().ToLower()));
			return this;
		}
		
		/// <summary>
		/// When the application searches for a node, it searches for nodes in specific tree types.
		/// If SyncTree is used, it will sync the tree nodes with the active tree type, therefore if
		/// a developer wants to sync a specific tree, they can call this method to set the type to sync.
		/// </summary>
		/// <remarks>
		/// Each branch of a particular tree should theoretically be the same type, however, developers can
		/// override the type of each branch in their BaseTree's but this is not standard practice. If there
		/// are multiple types of branches in one tree, then only those branches that have the Active tree type
		/// will be searched for syncing.
		/// </remarks>
		/// <param name="treeType"></param>
		/// <returns></returns>
		public ClientTools SetActiveTreeType(string treeType)
		{
			RegisterClientScript(string.Format(Scripts.SetActiveTreeType, treeType));
			return this;
		}

		/// <summary>
        /// Closes the Umbraco dialog window if it is open
		/// </summary>
		/// <param name="returnVal">specify a value to return to add to the onCloseCallback method if one was specified in the OpenModalWindow method</param>
		/// <returns></returns>
		public ClientTools CloseModalWindow(string returnVal)
		{
			RegisterClientScript(Scripts.CloseModalWindow(returnVal));
			return this;
		}
        /// <summary>
        /// Closes the umbraco dialog window if it is open
        /// </summary>
        /// <returns></returns>
        public ClientTools CloseModalWindow()
        {
            return CloseModalWindow("");
        }


	    /// <summary>
	    /// Opens a modal window
	    /// </summary>
	    /// <param name="url"></param>
	    /// <param name="name"></param>
	    /// <param name="height"></param>
	    /// <param name="showHeader"></param>
	    /// <param name="width"></param>
	    /// <param name="top"></param>
	    /// <param name="leftOffset"></param>
	    /// <param name="closeTriggers"></param>
	    /// <param name="onCloseCallback"></param>
	    /// <returns></returns>
	    public ClientTools OpenModalWindow(string url, string name, bool showHeader, int width, int height, int top, int leftOffset, string closeTriggers, string onCloseCallback)
		{
			RegisterClientScript(Scripts.OpenModalWindow(url, name, showHeader, width, height, top, leftOffset, closeTriggers, onCloseCallback));
			return this;
		}

        /// <summary>
        /// This will use the ScriptManager to register the script if one is available, otherwise will default to the ClientScript
        /// class of the page.
        /// </summary>
        /// <param name="script"></param>
        private void RegisterClientScript(string script)
		{
			//use the hash code of the script to generate the key, this way, the exact same script won't be
			//inserted more than once.
            if (ScriptManager.GetCurrent(_page) != null)
            {
                ScriptManager.RegisterStartupScript(_page, _page.GetType(), script.GetHashCode().ToString(), script, true);
            }
            else
            {
                _page.ClientScript.RegisterStartupScript(_page.GetType(), script.GetHashCode().ToString(), script, true);
            }
		}




	}
}
