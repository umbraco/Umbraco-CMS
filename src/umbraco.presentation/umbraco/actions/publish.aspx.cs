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

namespace umbraco.presentation.actions
{
    public partial class publish : BasePages.UmbracoEnsuredPage
    {
        private Document d;

        protected void Page_Load(object sender, EventArgs e)
        {
            d = new Document(int.Parse(helper.Request("id")));

            if (!base.ValidateUserApp("content"))
                throw new ArgumentException("The current user doesn't have access to this application. Please contact the system administrator.");

            if (!base.ValidateUserNodeTreePermissions(d.Path, "U"))
                throw new ArgumentException("The current user doesn't have permissions to publish this document. Please contact the system administrator.");
            
            pane_publish.Text = ui.Text("publish") + " '" + d.Text + "'";
            Panel2.Text = ui.Text("publish");
            warning.Text = ui.Text("publish") + " '" + d.Text + "'. " + ui.Text("areyousure");
            deleteButton.Text = ui.Text("publish");
        }

        protected void deleteButton_Click(object sender, EventArgs e)
        {
            deleteMessage.Visible = true;
            deleteMessage.Text = ui.Text("editContentPublishedHeader");

            confirm.Visible = false;
            d.Publish(getUser());
            library.UpdateDocumentCache(d.Id);

            deleted.Text = ui.Text("editContentPublishedHeader") + " ('" + d.Text + "') " + ui.Text("editContentPublishedText") + "</p><p><a href=\"" + library.NiceUrl(d.Id) + "\"> " + ui.Text("view") + " " + d.Text + "</a>";
        }
    }
}
