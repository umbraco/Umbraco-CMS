using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.UI.Pages;

namespace umbraco.presentation.actions
{
    public partial class delete : UmbracoEnsuredPage
    {
        private IContent c;

        protected void Page_Load(object sender, EventArgs e)
        {
            c = Current.Services.ContentService.GetById(int.Parse(Request.GetItemAsString("id")));

            if (Security.ValidateUserApp(Constants.Applications.Content) == false)
                throw new ArgumentException("The current user doesn't have access to this application. Please contact the system administrator.");
            if (Security.ValidateUserNodeTreePermissions(Security.CurrentUser, c.Path, "D") == false)
                throw new ArgumentException("The current user doesn't have permissions to delete this document. Please contact the system administrator.");

            pane_delete.Text = Services.TextService.Localize("delete") + " '" + c.Name + "'";
            Panel2.Text = Services.TextService.Localize("delete");
            warning.Text = Services.TextService.Localize("confirmdelete") + " '" + c.Name + "'";
            deleteButton.Text = Services.TextService.Localize("delete");
        }

        protected void deleteButton_Click(object sender, EventArgs e)
        {
            deleteMessage.Text = Services.TextService.Localize("deleted");
            deleted.Text =  "'" + c.Name + "' " + Services.TextService.Localize("deleted");
            deleteMessage.Visible = true;
            confirm.Visible = false;

            Current.Services.ContentService.Delete(c);
        }
    }
}
