using System;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.UI.Pages;

namespace umbraco.presentation.actions
{
    public partial class delete : UmbracoEnsuredPage
    {
        private Document d;

        protected void Page_Load(object sender, EventArgs e)
        {
            d = new Document(int.Parse(Request.GetItemAsString("id")));

            if (Security.ValidateUserApp(Constants.Applications.Content) == false)
                throw new ArgumentException("The current user doesn't have access to this application. Please contact the system administrator.");
            if (Security.ValidateUserNodeTreePermissions(UmbracoUser, d.Path, "D") == false)
                throw new ArgumentException("The current user doesn't have permissions to delete this document. Please contact the system administrator.");
            
            pane_delete.Text = Services.TextService.Localize("delete") + " '" + d.Text + "'";
            Panel2.Text = Services.TextService.Localize("delete");
            warning.Text = Services.TextService.Localize("confirmdelete") + " '" + d.Text + "'";
            deleteButton.Text = Services.TextService.Localize("delete");
        }

        protected void deleteButton_Click(object sender, EventArgs e)
        {
            deleteMessage.Text = Services.TextService.Localize("deleted");
            deleted.Text =  "'" + d.Text + "' " + Services.TextService.Localize("deleted");
            deleteMessage.Visible = true;
            confirm.Visible = false;
            
            d.delete();
        }
    }
}
