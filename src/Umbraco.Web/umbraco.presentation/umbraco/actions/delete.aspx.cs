using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;

namespace umbraco.presentation.actions
{
    public partial class delete : BasePages.UmbracoEnsuredPage
    {
        private Document d;

        protected void Page_Load(object sender, EventArgs e)
        {
            d = new Document(int.Parse(helper.Request("id")));

            if (!base.ValidateUserApp(Constants.Applications.Content))
                throw new ArgumentException("The current user doesn't have access to this application. Please contact the system administrator.");
            if (!base.ValidateUserNodeTreePermissions(d.Path, "D"))
                throw new ArgumentException("The current user doesn't have permissions to delete this document. Please contact the system administrator.");
            
            pane_delete.Text = ui.Text("delete") + " '" + d.Text + "'";
            Panel2.Text = ui.Text("delete");
            warning.Text = ui.Text("confirmdelete") + " '" + d.Text + "'";
            deleteButton.Text = ui.Text("delete");
        }

        protected void deleteButton_Click(object sender, EventArgs e)
        {
            deleteMessage.Text = ui.Text("deleted");
            deleted.Text =  "'" + d.Text + "' " + ui.Text("deleted");
            deleteMessage.Visible = true;
            confirm.Visible = false;
            
            d.delete();
        }
    }
}
