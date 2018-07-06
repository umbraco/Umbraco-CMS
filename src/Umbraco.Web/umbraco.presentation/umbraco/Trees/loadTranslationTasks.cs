using Umbraco.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;
using Umbraco.Web._Legacy.Actions;
using Umbraco.Web._Legacy.BusinessLogic;

namespace umbraco {
    public class loadOpenTasks : BaseTree {

        public loadOpenTasks(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode) {
            rootNode.Action = "javascript:openTranslationOverview(" + CurrentUser().Id + ",'open');";
            rootNode.Text = Services.TextService.Localize("translation/assignedTasks");
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

        private IUser CurrentUser()
        {
            return UmbracoContext.Current.Security.CurrentUser;
        }

        public override void Render(ref XmlTree tree) {
            foreach (Task t in Task.GetTasks(CurrentUser(), false)) {

                if (t.Type.Alias == "toTranslate") {
                    XmlTreeNode xNode = XmlTreeNode.Create(this);
                    xNode.Menu.Clear();

                    xNode.NodeID = t.Id.ToString();
                    xNode.Text = t.TaskEntityEntity.Name;
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
            rootNode.Action = "javascript:openTranslationOverview(" + CurrentUser().Id + ", 'owned');";
            rootNode.Text = Services.TextService.Localize("translation/ownedTasks");
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

        private IUser CurrentUser()
        {
            return UmbracoContext.Current.Security.CurrentUser;
        }

        public override void Render(ref XmlTree tree) {
            foreach (Task t in Task.GetOwnedTasks(CurrentUser(), false)) {

                if (t.Type.Alias == "toTranslate") {
                    XmlTreeNode xNode = XmlTreeNode.Create(this);
                    xNode.Menu.Clear();
                    xNode.NodeID = t.Id.ToString();
                    xNode.Text = t.TaskEntityEntity.Name;
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
