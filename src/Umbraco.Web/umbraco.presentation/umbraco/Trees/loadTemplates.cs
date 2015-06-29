using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.contentitem;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Services;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.cms.businesslogic.template;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;


namespace umbraco
{
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    public class loadTemplates : BaseTree
    {
        public loadTemplates(string application) : base(application) {}

        private ViewHelper _viewHelper = new ViewHelper(new PhysicalFileSystem(SystemDirectories.MvcViews));

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.NodeType = "init" + TreeAlias; 
			rootNode.NodeID = "init";
        }


        public override void RenderJS(ref StringBuilder Javascript)
        {
           
            Javascript.Append(
                @"
                function openTemplate(id) {
	                UmbClientMgr.contentFrame('settings/editTemplate.aspx?templateID=' + id);
                }   
                
                 function openView(id) {
                    UmbClientMgr.contentFrame('settings/views/editView.aspx?treeType=templates&templateID=' + id);
			    }

                function openSkin(id) {
	                UmbClientMgr.contentFrame('settings/editSkin.aspx?skinID=' + id);
                }
                ");
        }


        public override void Render(ref XmlTree tree)
        {
            string folder = umbraco.library.Request("folder");
            string folderPath = umbraco.library.Request("folderPath");

            if (!string.IsNullOrEmpty(folder))
                RenderTemplateFolderItems(folder, folderPath, ref tree);
            else
            {
                if (UmbracoConfig.For.UmbracoSettings().Templates.EnableTemplateFolders)
                    RenderTemplateFolders(ref tree);
                
                RenderTemplates(ref tree);
            }
        }

        private void RenderTemplateFolderItems(string folder, string folderPath, ref XmlTree tree)
        {
            string relPath = SystemDirectories.Masterpages + "/" + folder;
            if (!string.IsNullOrEmpty(folderPath))
                relPath += folderPath;

            string fullPath = IOHelper.MapPath(relPath);

            foreach (string dir in System.IO.Directory.GetDirectories(fullPath))
            {
                System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(dir);
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.Menu.Clear();
                xNode.Menu.Add(ActionRefresh.Instance);
                xNode.NodeID = "-1";
                xNode.Text = directoryInfo.Name;
                xNode.HasChildren = true;
                xNode.Icon = "icon-folder";
                xNode.OpenIcon = "icon-folder";
                xNode.Source = GetTreeServiceUrl(directoryInfo.Name) + "&folder=" + folder + "&folderPath=" + folderPath + "/" + directoryInfo.Name;
                tree.Add(xNode);
            }

            foreach (string file in System.IO.Directory.GetFiles(fullPath))
            {
                System.IO.FileInfo fileinfo = new FileInfo(file);
                string ext = fileinfo.Extension.ToLower().Trim('.');
               
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.Menu.Clear();
                xNode.Menu.Add(ActionRefresh.Instance);
                xNode.NodeID = "-1";
                xNode.Text = Path.GetFileName(file);
                xNode.HasChildren = false;
                xNode.Action = "javascript:openScriptEditor('" + relPath + "/" + Path.GetFileName(file) + "');";

                //tree.Add(xNode);
               
                
                switch (ext)
                {
                    case "master":
                         xNode.Icon = "icon-newspaper-alt";
                         xNode.OpenIcon = "icon-newspaper-alt";
                         tree.Add(xNode);
                        break;
                    case "css":
                    case "js":
                        xNode.Icon = "icon-brackets";
                        xNode.OpenIcon = "icon-brackets";
                        tree.Add(xNode);
                        break;
                    case "xml":
                        if (xNode.Text == "skin.xml")
                        {
                            xNode.Icon = "icon-code";
                            xNode.OpenIcon = "icon-code";
                            tree.Add(xNode);
                        }
                        break;
                    default:
                        break;
                }

                       
                
                //xNode.Source = GetTreeServiceUrl(s.Alias) + "&skin=" + skin + "&path=" + path;
           }
            
            
        }

        private void RenderTemplateFolders(ref XmlTree tree)
        {
            if (base.m_id == -1)
            {
                foreach (string s in Directory.GetDirectories(IOHelper.MapPath(SystemDirectories.Masterpages)))
                {
                    var _s = Path.GetFileNameWithoutExtension(s);

                    XmlTreeNode xNode = XmlTreeNode.Create(this);
                    xNode.NodeID = _s;
                    xNode.Text = _s;
                    xNode.Icon = "icon-folder";
                    xNode.OpenIcon = "icon-folder";
                    xNode.Source = GetTreeServiceUrl(_s) + "&folder=" + _s;
                    xNode.HasChildren = true;
                    xNode.Menu.Clear();
                    xNode.Menu.Add(ActionRefresh.Instance);

                    OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    if (xNode != null)
                    {
                        tree.Add(xNode);
                        OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    }
                }
            }
        }

        private void RenderTemplates(ref XmlTree tree)
        {
            List<Template> templates = null;
            if (m_id == -1)
                templates = Template.GetAllAsList().FindAll(t => t.HasMasterTemplate == false);
            else
                templates = Template.GetAllAsList().FindAll(t => t.MasterTemplate == m_id);

            foreach (Template t in templates)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = t.Id.ToString(CultureInfo.InvariantCulture);
                xNode.Text = t.Text;
                xNode.Source = GetTreeServiceUrl(t.Id);
                xNode.HasChildren = t.HasChildren;

                if (UmbracoConfig.For.UmbracoSettings().Templates.DefaultRenderingEngine == RenderingEngine.Mvc && _viewHelper.ViewExists(t.TemplateEntity))
                {
                    xNode.Action = "javascript:openView(" + t.Id + ");";
                    xNode.Icon = "icon-newspaper-alt";
                    xNode.OpenIcon = "icon-newspaper-alt";
                }
				else
				{
                    xNode.Action = "javascript:openTemplate(" + t.Id + ");";
                    xNode.Icon = "icon-newspaper-alt";
                    xNode.OpenIcon = "icon-newspaper-alt";
                }
                
                if (t.HasChildren)
                {
                    xNode.Icon = "icon-newspaper";
                    xNode.OpenIcon = "icon-newspaper";
                    //do not show the delete option if it has children
                    xNode.Menu.Remove(ActionDelete.Instance);
                }

                OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                
                if (xNode != null)
                {
                    tree.Add(xNode);
                    OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                }

            }

        }


        protected override void CreateAllowedActions(ref List<IAction> actions) {
            actions.Clear();
            actions.AddRange(new IAction[] { ActionNew.Instance, ActionDelete.Instance, 
                ContextMenuSeperator.Instance, ActionRefresh.Instance });
        }
    }
    
}
