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
using umbraco.cms.businesslogic.translation;
using umbraco.cms.businesslogic.task;

using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.BusinessLogic.Actions;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;

namespace umbraco {
    public class loadOpenTasks : BaseTree {

        public loadOpenTasks(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode) {
            rootNode.Action = "javascript:openTranslationOverview(" + currentUser().Id + ",'open');";
            rootNode.Text = ui.Text("translation", "assignedTasks");
        }

        protected override void CreateRootNodeActions(ref List<IAction> actions) {
            actions.Clear();
            actions.Add(ActionRefresh.Instance);
        }

        public override void RenderJS(ref StringBuilder Javascript) {
           Javascript.Append(@"
                    function openTranslationTask(id) {
	                    UmbClientMgr.contentFrame('translation/details.aspx?id=' + id);
                    }
                    function openTranslationOverview(id, mode) {
	                    UmbClientMgr.contentFrame('translation/default.aspx?id=' + id + '&mode=' + mode);
                    }
                    ");     
        }

        private User currentUser() {
            return User.GetCurrent();
        }
        public override void Render(ref XmlTree tree) {
            foreach (Task t in Task.GetTasks(currentUser(), false)) {
                
                if (t.Type.Alias == "toTranslate") {
                    XmlTreeNode xNode = XmlTreeNode.Create(this);
                    xNode.Menu.Clear();

                    xNode.NodeID = t.Id.ToString();
                    xNode.Text = t.Node.Text;
                    xNode.Action = "javascript:openTranslationTask(" + t.Id.ToString() + ")";
                    xNode.Icon = ".sprTreeSettingLanguage";
                    xNode.OpenIcon = ".sprTreeSettingLanguage";

                    OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    if (xNode != null)
                    {
                        tree.Add(xNode);
                        OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    }
                    
                }

            }
        }        
    }

    public class loadYourTasks : BaseTree {

        public loadYourTasks(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode) {
            rootNode.Action = "javascript:openTranslationOverview(" + currentUser().Id + ", 'owned');";
            rootNode.Text = ui.Text("translation", "ownedTasks");
        }

        protected override void CreateRootNodeActions(ref List<IAction> actions) {
            actions.Clear();
            actions.Add(ActionRefresh.Instance);
        }

        public override void RenderJS(ref StringBuilder Javascript) {
            Javascript.Append(@"
                    function openTranslationTask(id) {
	                    UmbClientMgr.contentFrame('translation/details.aspx?id=' + id);
                    }");
        }

        private User currentUser()
        {
            return User.GetCurrent();
        }

        public override void Render(ref XmlTree tree) {
            foreach (Task t in Task.GetOwnedTasks(currentUser(), false)) {

                if (t.Type.Alias == "toTranslate") {
                    XmlTreeNode xNode = XmlTreeNode.Create(this);
                    xNode.Menu.Clear();
                    xNode.NodeID = t.Id.ToString();
                    xNode.Text = t.Node.Text;
                    xNode.Action = "javascript:openTranslationTask(" + t.Id.ToString() + ")";
                    xNode.Icon = ".sprTreeSettingLanguage";
                    xNode.OpenIcon = ".sprTreeSettingLanguage";

                    OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    if (xNode != null)
                    {
                        tree.Add(xNode);
                        OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    }
                    
                }

            }
        }
    }
    

}
