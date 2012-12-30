using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.template;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;

namespace Umbraco.Web.Trees
{
	[Tree("settings", "partialViews", "Partial Views", sortOrder: 2)]
	public class PartialViewsTree : FileSystemTree
	{
		public PartialViewsTree(string application) : base(application) { }

		public override void RenderJS(ref StringBuilder javascript)
		{
			javascript.Append(
				@"
		                 function openPartialView(id) {
		                    UmbClientMgr.contentFrame('Settings/Views/EditView.aspx?file=' + id);
					    }
		                ");
		}

		protected override void CreateRootNode(ref XmlTreeNode rootNode)
		{
			rootNode.NodeType = "init" + TreeAlias;
			rootNode.NodeID = "init";
		}

		protected override string FilePath
		{
			get { return SystemDirectories.MvcViews + "/Partials/"; }
		}

		protected override string FileSearchPattern
		{
			get { return "*.*"; }
		}

		//protected override void OnRenderFolderNode(ref XmlTreeNode xNode)
		//{
		//	xNode.Menu = new List<IAction>(new IAction[] { ActionDelete.Instance, ContextMenuSeperator.Instance, ActionNew.Instance, ContextMenuSeperator.Instance, ActionRefresh.Instance });
		//	xNode.NodeType = "scriptsFolder";
		//}

		protected override void OnRenderFileNode(ref XmlTreeNode xNode)
		{
			xNode.Action = xNode.Action.Replace("openFile", "openPartialView");
			xNode.Icon = "settingsScript.gif";
			xNode.OpenIcon = "settingsScript.gif";
		}

//		protected override void CreateRootNode(ref XmlTreeNode rootNode)
//		{
//			rootNode.NodeType = "init" + TreeAlias;
//			rootNode.NodeID = "init";
//		}

//		private string _partialViewsFolder = SystemDirectories.MvcViews + "/Partials/";

//		public override void RenderJS(ref StringBuilder Javascript)
//		{

//			Javascript.Append(
//				@"
//                 function openView(id) {
//                    UmbClientMgr.contentFrame('settings/views/editView.aspx?templateID=' + id);
//			    }
//                ");
//		}


//		public override void Render(ref XmlTree tree)
//		{
//			string folder = global::umbraco.library.Request("folder");
//			string folderPath = global::umbraco.library.Request("folderPath");

//			if (!string.IsNullOrEmpty(folder))
//				RenderTemplateFolderItems(folder, folderPath, ref tree);
//			else
//			{
//				RenderTemplates(ref tree);
//			}
//		}

//		private void RenderTemplateFolderItems(string folder, string folderPath, ref XmlTree tree)
//		{
//			string relPath = _partialViewsFolder + folder;
//			if (!string.IsNullOrEmpty(folderPath))
//				relPath += folderPath;

//			string fullPath = IOHelper.MapPath(relPath);

//			foreach (string dir in System.IO.Directory.GetDirectories(fullPath))
//			{
//				System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(dir);
//				XmlTreeNode xNode = XmlTreeNode.Create(this);
//				xNode.Menu.Clear();
//				xNode.Menu.Add(ActionRefresh.Instance);
//				xNode.NodeID = "-1";
//				xNode.Text = directoryInfo.Name;
//				xNode.HasChildren = true;
//				xNode.Icon = "folder.gif";
//				xNode.OpenIcon = "folder_o.gif";
//				xNode.Source = GetTreeServiceUrl(directoryInfo.Name) + "&folder=" + folder + "&folderPath=" + folderPath + "/" + directoryInfo.Name;
//				tree.Add(xNode);
//			}

//			foreach (string file in System.IO.Directory.GetFiles(fullPath))
//			{
//				System.IO.FileInfo fileinfo = new FileInfo(file);
//				string ext = fileinfo.Extension.ToLower().Trim('.');

//				XmlTreeNode xNode = XmlTreeNode.Create(this);
//				xNode.Menu.Clear();
//				xNode.Menu.Add(ActionRefresh.Instance);
//				xNode.NodeID = "-1";
//				xNode.Text = Path.GetFileName(file);
//				xNode.HasChildren = false;
//				xNode.Action = "javascript:openScriptEditor('" + relPath + "/" + Path.GetFileName(file) + "');";

//				//tree.Add(xNode);


//				switch (ext)
//				{
//					case "master":
//						xNode.Icon = "settingTemplate.gif";
//						xNode.OpenIcon = "settingTemplate.gif";
//						tree.Add(xNode);
//						break;
//					case "css":
//					case "js":
//						xNode.Icon = "settingsScript.gif";
//						xNode.OpenIcon = "settingsScript.gif";
//						tree.Add(xNode);
//						break;
//					case "xml":
//						if (xNode.Text == "skin.xml")
//						{
//							xNode.Icon = "settingXml.gif";
//							xNode.OpenIcon = "settingXml.gif";
//							tree.Add(xNode);
//						}
//						break;
//					default:
//						break;
//				}



//				//xNode.Source = GetTreeServiceUrl(s.Alias) + "&skin=" + skin + "&path=" + path;
//			}


//		}

//		private void RenderTemplateFolders(ref XmlTree tree)
//		{
//			if (base.m_id == -1)
//			{
//				foreach (string s in Directory.GetDirectories(IOHelper.MapPath(_partialViewsFolder)))
//				{
//					var _s = Path.GetFileNameWithoutExtension(s);

//					XmlTreeNode xNode = XmlTreeNode.Create(this);
//					xNode.NodeID = _s;
//					xNode.Text = _s;
//					xNode.Icon = "folder.gif";
//					xNode.OpenIcon = "folder_o.gif";
//					xNode.Source = GetTreeServiceUrl(_s) + "&folder=" + _s;
//					xNode.HasChildren = true;
//					xNode.Menu.Clear();
//					xNode.Menu.Add(ActionRefresh.Instance);

//					OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
//					if (xNode != null)
//					{
//						tree.Add(xNode);
//						OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
//					}
//				}
//			}
//		}

//		private void RenderTemplates(ref XmlTree tree)
//		{
//			List<Template> templates = null;
//			if (base.m_id == -1)
//				templates = Template.GetAllAsList().FindAll(delegate(Template t) { return !t.HasMasterTemplate; });
//			else
//				templates = Template.GetAllAsList().FindAll(delegate(Template t) { return t.MasterTemplate == base.m_id; });

//			foreach (Template t in templates)
//			{
//				XmlTreeNode xNode = XmlTreeNode.Create(this);
//				xNode.NodeID = t.Id.ToString();
//				xNode.Text = t.Text;
//				xNode.Source = GetTreeServiceUrl(t.Id);
//				xNode.HasChildren = t.HasChildren;

//				if (Umbraco.Core.Configuration.UmbracoSettings.DefaultRenderingEngine == RenderingEngine.Mvc && ViewHelper.ViewExists(t))
//				{
//					xNode.Action = "javascript:openView(" + t.Id + ");";
//					xNode.Icon = "settingView.gif";
//					xNode.OpenIcon = "settingView.gif";
//				}
//				else
//				{
//					xNode.Action = "javascript:openTemplate(" + t.Id + ");";
//					xNode.Icon = "settingTemplate.gif";
//					xNode.OpenIcon = "settingTemplate.gif";
//				}

//				if (t.HasChildren)
//				{
//					xNode.Icon = "settingMasterTemplate.gif";
//					xNode.OpenIcon = "settingMasterTemplate.gif";
//				}

//				OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);

//				if (xNode != null)
//				{
//					tree.Add(xNode);
//					OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
//				}

//			}

//		}


//		protected override void CreateAllowedActions(ref List<IAction> actions)
//		{
//			actions.Clear();
//			actions.AddRange(new IAction[] { ActionNew.Instance, ActionDelete.Instance, 
//				ContextMenuSeperator.Instance, ActionRefresh.Instance });
//		}
		
	}
}
