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
using umbraco.cms.businesslogic.template;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.skinning;

namespace umbraco
{
    public class loadTemplates : BaseTree
    {
        public loadTemplates(string application) : base(application) { }

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
                if(UmbracoSettings.EnableTemplateFolders)
                    RenderTemplateFolders(ref tree);
                
                RenderTemplates(ref tree);
            }
        }


        private void RenderTemplateFolderItems(string folder, string folderPath, ref XmlTree tree)
        {
            string relPath = IO.SystemDirectories.Masterpages + "/" + folder;
            if (!string.IsNullOrEmpty(folderPath))
                relPath += folderPath;

            string fullPath = IO.IOHelper.MapPath(relPath);

            foreach (string dir in System.IO.Directory.GetDirectories(fullPath))
            {
                System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(dir);
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.Menu.Clear();
                xNode.Menu.Add(ActionRefresh.Instance);
                xNode.NodeID = "-1";
                xNode.Text = directoryInfo.Name;
                xNode.HasChildren = true;
                xNode.Icon = "folder.gif";
                xNode.OpenIcon = "folder_o.gif";
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
                         xNode.Icon = "settingTemplate.gif";
                         xNode.OpenIcon = "settingTemplate.gif";
                         tree.Add(xNode);
                        break;
                    case "css":
                    case "js":
                        xNode.Icon = "settingsScript.gif";
                        xNode.OpenIcon = "settingsScript.gif";
                        tree.Add(xNode);
                        break;
                    case "xml":
                        if (xNode.Text == "skin.xml")
                        {
                            xNode.Icon = "settingXml.gif";
                            xNode.OpenIcon = "settingXml.gif";
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
                foreach (string s in Directory.GetDirectories(IO.IOHelper.MapPath(IO.SystemDirectories.Masterpages)))
                {
                    var _s = Path.GetFileNameWithoutExtension(s);

                    XmlTreeNode xNode = XmlTreeNode.Create(this);
                    xNode.NodeID = _s;
                    xNode.Text = _s;
                    xNode.Icon = "folder.gif";
                    xNode.OpenIcon = "folder_o.gif";
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
            if (base.m_id == -1)
                templates = Template.GetAllAsList().FindAll(delegate(Template t) { return !t.HasMasterTemplate; });
            else
                templates = Template.GetAllAsList().FindAll(delegate(Template t) { return t.MasterTemplate == base.m_id; });

            foreach (Template t in templates)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = t.Id.ToString();
                xNode.Text = t.Text;
                xNode.Action = "javascript:openTemplate(" + t.Id + ");";
                xNode.Icon = "settingTemplate.gif";
                xNode.OpenIcon = "settingTemplate.gif";
                xNode.Source = GetTreeServiceUrl(t.Id);
                xNode.HasChildren = t.HasChildren;
                if (t.HasChildren)
                {
                    xNode.Icon = "settingMasterTemplate.gif";
                    xNode.OpenIcon = "settingMasterTemplate.gif";
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
