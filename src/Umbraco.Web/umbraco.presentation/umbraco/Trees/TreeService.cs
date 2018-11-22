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
using Umbraco.Core.IO;
using umbraco.uicontrols;

namespace umbraco.cms.presentation.Trees
{

	/// <summary>
	/// A utility class to aid in creating the URL for returning XML for a tree structure and
	/// for reading the parameters from the URL when a request is made.
	/// </summary>
	public class TreeService : TreeUrlGenerator, ITreeService
	{

		/// <summary>
		/// Default empty constructor
		/// </summary>
        public TreeService() : base() { }

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
            StartNodeID = startNodeID ?? -1;
			TreeType = treeType;
            ShowContextMenu = showContextMenu ?? true;
            IsDialog = isDialog ?? false;
			m_dialogMode = dialogMode;
			App = app;
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
            StartNodeID = startNodeID ?? -1;
			TreeType = treeType;
            ShowContextMenu = showContextMenu ?? true;
            IsDialog = isDialog ?? false;
			m_dialogMode = dialogMode;
			App = app;
			NodeKey = nodeKey;
		}

		public TreeService(int? startNodeID, string treeType, bool? showContextMenu,
			bool? isDialog, TreeDialogModes dialogMode, string app, string nodeKey, string functionToCall)
		{
			StartNodeID = startNodeID ?? -1;
			TreeType = treeType;
			ShowContextMenu = showContextMenu ?? true;
			IsDialog = isDialog ?? false;
			m_dialogMode = dialogMode;
			App = app;
			NodeKey = nodeKey;
			FunctionToCall = functionToCall;
		}

        private TreeDialogModes m_dialogMode;

		#region Public Properties


		public TreeDialogModes DialogMode
		{
			get { return m_dialogMode; }
			set { m_dialogMode = value; }
		}

		#endregion

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

        protected override string GetUrl(string pageUrl)
        {
            StringBuilder sb = new StringBuilder(base.GetUrl(pageUrl));
            if (this.DialogMode != TreeDialogModes.none) sb.Append(string.Format("&dialogMode={0}", this.DialogMode.ToString()));
            return sb.ToString();
        }

	}
}
