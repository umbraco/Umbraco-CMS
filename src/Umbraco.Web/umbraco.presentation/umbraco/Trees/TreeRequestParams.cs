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

namespace umbraco.cms.presentation.Trees
{
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
                p.Add(key, HttpUtility.HtmlEncode(HttpContext.Current.Request.QueryString[key]));
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

		/// <summary>
		/// Converts the tree parameters to a tree service object
		/// </summary>
		/// <returns></returns>
		public TreeService CreateTreeService()
		{
			return new TreeService()
			{
				ShowContextMenu = this.ShowContextMenu,
				IsDialog = this.IsDialog,
				DialogMode = this.DialogMode,
				App = this.Application,
				TreeType = this.TreeType,
				NodeKey = this.NodeKey,
				StartNodeID = this.StartNodeID,
				FunctionToCall = this.FunctionToCall				
			};
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
				if(m_params.ContainsKey("id"))
                {
				    int sNodeID;
				    if (int.TryParse(m_params["id"], out sNodeID))
					    return sNodeID;
                }
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
