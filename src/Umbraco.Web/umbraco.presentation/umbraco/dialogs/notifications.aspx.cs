using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.workflow;
using umbraco.interfaces;
using Umbraco.Core;

namespace umbraco.dialogs
{
    /// <summary>
    /// Summary description for cruds.
    /// </summary>
    public partial class notifications : UmbracoEnsuredPage
    {
        private ArrayList actions = new ArrayList();
        private CMSNode node;

        public notifications()
        {
            CurrentApp = BusinessLogic.DefaultApps.content.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Button1.Text = ui.Text("update");
            pane_form.Text = ui.Text("notifications", "editNotifications", Server.HtmlEncode(node.Text), base.getUser());
        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);

            node = new CMSNode(int.Parse(helper.Request("id")));

            var actionList = ActionsResolver.Current.Actions;

            foreach (IAction a in actionList.OrderBy(x=>x.Alias))
            {
                if (a.ShowInNotifier)
                {
                    CheckBox c = new CheckBox();
                    c.ID = a.Letter.ToString(CultureInfo.InvariantCulture);
                    
                    if (UmbracoUser.GetNotifications(node.Path).IndexOf(a.Letter) > -1)
                        c.Checked = true;

                    uicontrols.PropertyPanel pp = new uicontrols.PropertyPanel();
                    pp.CssClass = "inline notifications";
                    pp.Text = ui.Text("actions", a.Alias);
                    pp.Controls.Add(c);

                    pane_form.Controls.Add(pp);
                    
                    actions.Add(c);
                }
            }
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion

        protected void Button1_Click(object sender, EventArgs e)
        {
            string notifications = "";

            // First off - load all users
            foreach (CheckBox c in actions)
            {
                // Update the user with the new permission
                if (c.Checked)
                    notifications += c.ID;
            }
            Notification.UpdateNotifications(UmbracoUser, node, notifications);
            UmbracoUser.resetNotificationCache();
            UmbracoUser.initNotifications();

            var feedback = new uicontrols.Feedback();
            feedback.Text = ui.Text("notifications") + " " + ui.Text("ok") + "</p><p><a href='#' class='btn btn-primary' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
            feedback.type = uicontrols.Feedback.feedbacktype.success;

            pane_form.Controls.Clear();
            pane_form.Controls.Add(feedback);

            pl_buttons.Visible = false;
        }
    }
}