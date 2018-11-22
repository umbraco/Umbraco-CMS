using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.uicontrols
{
    /// <summary>
    /// This class will generate the URLs for iframe tree pages. 
    /// Generally used to get the a tree picker url.    
    /// </summary>
    /// <remarks>
    /// This was created in 4.1 so that this helper class can be exposed to other assemblies since
    /// it only existed in the presentation assembly in previous versions
    /// </remarks>
    public class TreeUrlGenerator
    {

		public const string TREE_URL = "tree.aspx";
		public const string INIT_URL = "treeinit.aspx";
		public const string PICKER_URL = "treepicker.aspx";

		private int? m_startNodeID;
		private string m_treeType;
		private bool? m_showContextMenu;
		private bool? m_isDialog;
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
            return Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) + "/" + GetUrl(TREE_URL);
		}

		/// <summary>
		/// Static method to return the tree service url with the specified parameters
		/// </summary>
		/// <param name="startNodeID"></param>
		/// <param name="treeType"></param>
		/// <param name="showContextMenu"></param>
		/// <param name="isDialog"></param>
		/// <param name="app"></param>
		/// <param name="nodeKey"></param>
		/// <param name="functionToCall"></param>
		/// <returns></returns>
		public static string GetServiceUrl(int? startNodeID, string treeType, bool? showContextMenu,
			bool? isDialog, string app, string nodeKey, string functionToCall)
		{
            TreeUrlGenerator treeSvc = new TreeUrlGenerator()
            {
                StartNodeID = startNodeID ?? -1,
                TreeType = treeType,
                ShowContextMenu = showContextMenu ?? true,
                IsDialog = isDialog ?? false,
                App = app,
                NodeKey = nodeKey,
                FunctionToCall = functionToCall
            };
			return treeSvc.GetServiceUrl();
		}

		/// <summary>
		/// Returns the url for initializing the tree based on the parameters specified on this class
		/// </summary>
		/// <returns></returns>
		public string GetInitUrl()
		{
            return Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) + "/" + GetUrl(INIT_URL);
		}

		/// <summary>
		/// static method to return the tree init url with the specified parameters
		/// </summary>
		/// <param name="startNodeID"></param>
		/// <param name="treeType"></param>
		/// <param name="showContextMenu"></param>
		/// <param name="isDialog"></param>
		/// <param name="app"></param>
		/// <param name="nodeKey"></param>
		/// <param name="functionToCall"></param>
		/// <returns></returns>
		public static string GetInitUrl(int? startNodeID, string treeType, bool? showContextMenu,
		   bool? isDialog, string app, string nodeKey, string functionToCall)
		{
            TreeUrlGenerator treeSvc = new TreeUrlGenerator()
            {
                StartNodeID = startNodeID ?? -1,
                TreeType = treeType,
                ShowContextMenu = showContextMenu ?? true,
                IsDialog = isDialog ?? false,
                App = app,
                NodeKey = nodeKey,
                FunctionToCall = functionToCall
            };
			return treeSvc.GetInitUrl();
		}

        /// <summary>
        /// Returns the url for the tree picker (used on modal windows) based on the parameters specified on this class
        /// </summary>
        public static string GetPickerUrl(string app, string treeType)
        {
            TreeUrlGenerator treeSvc = new TreeUrlGenerator();
            treeSvc.App = app;
            treeSvc.TreeType = treeType;
            return treeSvc.GetPickerUrl();
        }

        /// <summary>
        /// Returns the url for the tree picker (used on modal windows) based on the parameters specified on this class
        /// </summary>
        public string GetPickerUrl()
        {
            return Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) + "/dialogs/" + GetUrl(PICKER_URL);
        }

        [Obsolete("No longer used as useSubModal no longer has any relavence")]
		public static string GetPickerUrl(bool useSubModal, string app, string treeType)
		{
            return GetPickerUrl(app, treeType);
		}
        [Obsolete("No longer used as useSubModal no longer has any relavence")]
		public string GetPickerUrl(bool useSubModal)
		{
            return GetPickerUrl();
		}

		/// <summary>
		/// Generates the URL parameters for the tree service.
		/// </summary>
		/// <param name="pageUrl">the base url (i.e. tree.aspx)</param>
		/// <returns></returns>
		protected virtual string GetUrl(string pageUrl)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(pageUrl);
			//insert random
			sb.Append(string.Format("?rnd={0}", Guid.NewGuid().ToString("N")));

			sb.Append(string.Format("&id={0}", this.StartNodeID.ToString()));
			if (!string.IsNullOrEmpty(this.TreeType)) sb.Append(string.Format("&treeType={0}", this.TreeType));
			if (!string.IsNullOrEmpty(this.NodeKey)) sb.Append(string.Format("&nodeKey={0}", this.NodeKey));
			sb.Append(string.Format("&contextMenu={0}", this.ShowContextMenu.ToString().ToLower()));
			sb.Append(string.Format("&isDialog={0}", this.IsDialog.ToString().ToLower()));
			if (!string.IsNullOrEmpty(this.App)) sb.Append(string.Format("&app={0}", this.App));

			return sb.ToString();
		}

    }
}
