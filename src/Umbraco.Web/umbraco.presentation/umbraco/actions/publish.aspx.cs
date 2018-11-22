using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Umbraco.Core.Publishing;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;

namespace umbraco.presentation.actions
{
    public partial class publish : BasePages.UmbracoEnsuredPage
    {
        private Document d;

        protected void Page_Load(object sender, EventArgs e)
        {
            d = new Document(int.Parse(helper.Request("id")));

            if (!base.ValidateUserApp(Constants.Applications.Content))
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

            var result = d.SaveAndPublishWithResult(UmbracoUser);
            if (result.Success)
            {
                deleted.Text = ui.Text("editContentPublishedHeader") + " ('" + d.Text + "') " + ui.Text("editContentPublishedText") + "</p><p><a href=\"" + library.NiceUrl(d.Id) + "\"> " + ui.Text("view") + " " + d.Text + "</a>";
            }
            else
            {
                deleted.Text = "<div class='error' style='padding:10px'>" + GetMessageForStatus(result.Result) +  "</div>";
            }
            
        }

        private string GetMessageForStatus(PublishStatus status)
        {
            switch (status.StatusType)
            {
                case PublishStatusType.Success:
                case PublishStatusType.SuccessAlreadyPublished:
                    return ui.Text("speechBubbles", "editContentPublishedText", UmbracoUser);
                case PublishStatusType.FailedPathNotPublished:
                    return ui.Text("publish", "contentPublishedFailedByParent",
                                   string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id),
                                   UmbracoUser).Trim();
                case PublishStatusType.FailedCancelledByEvent:
                    return ui.Text("speechBubbles", "contentPublishedFailedByEvent");
                case PublishStatusType.FailedHasExpired:
                case PublishStatusType.FailedAwaitingRelease:
                case PublishStatusType.FailedIsTrashed:
                    return "Cannot publish document with a status of " + status.StatusType;
                case PublishStatusType.FailedContentInvalid:
                    return ui.Text("publish", "contentPublishedFailedInvalid",
                                   new[]
                                       {
                                           string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id),
                                           string.Join(",", status.InvalidProperties.Select(x => x.Alias))
                                       }, UmbracoUser);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
