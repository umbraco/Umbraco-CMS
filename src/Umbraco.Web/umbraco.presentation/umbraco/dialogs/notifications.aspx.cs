using Umbraco.Core.Services;
using System;
using System.Collections;
using System.Linq;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Web;
using Umbraco.Web.UI.Pages;
using Umbraco.Web._Legacy.Actions;

namespace umbraco.dialogs
{
    /// <summary>
    /// Summary description for cruds.
    /// </summary>
    public partial class notifications : UmbracoEnsuredPage
    {
        private ArrayList actions = new ArrayList();
        private IUmbracoEntity node;

        public notifications()
        {
            CurrentApp = Constants.Applications.Content.ToString();

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Button1.Text = Services.TextService.Localize("update");
            pane_form.Text = Services.TextService.Localize("notifications/editNotifications", new[] { node.Name});
        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);

            node = Services.EntityService.Get(int.Parse(Request.GetItemAsString("id")));

            var actionList = ActionsResolver.Current.Actions;
            
            foreach (var a in actionList)
            {
                if (a.ShowInNotifier)
                {
                   
                    CheckBox c = new CheckBox();
                    c.ID = a.Letter.ToString();

                    var notifications = Services.NotificationService.GetUserNotifications(Security.CurrentUser, node.Path);
                    if (notifications.Any(x => x.Action == a.Letter.ToString()))
                        c.Checked = true;

                    uicontrols.PropertyPanel pp = new umbraco.uicontrols.PropertyPanel();
                    pp.CssClass = "inline";
                    pp.Text = Services.TextService.Localize("actions", a.Alias);
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

            ApplicationContext.Current.Services.NotificationService.SetNotifications(Security.CurrentUser, node, notifications.ToCharArray().Select(x => x.ToString()).ToArray());
            
            var feedback = new umbraco.uicontrols.Feedback();
            feedback.Text = Services.TextService.Localize("notifications") + " " + Services.TextService.Localize("ok") + "</p><p><a href='#' class='btn btn-primary' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + Services.TextService.Localize("closeThisWindow") + "</a>";
            feedback.type = umbraco.uicontrols.Feedback.feedbacktype.success;

            pane_form.Controls.Clear();
            pane_form.Controls.Add(feedback);

            //pane_form.Visible = false;
            pl_buttons.Visible = false;
        }
    }
}