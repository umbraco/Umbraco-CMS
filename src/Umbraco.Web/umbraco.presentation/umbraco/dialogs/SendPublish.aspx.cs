using System;
using Umbraco.Web;
using Umbraco.Core;
using Umbraco.Web._Legacy.Actions;

namespace umbraco.dialogs
{
    /// <summary>
    /// Runs all action handlers for the ActionToPublish action for the document with
    /// the corresponding document id passed in by query string
    /// </summary>
    public partial class SendPublish : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        public SendPublish()
        {
            CurrentApp = Constants.Applications.Content.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["id"]))
            {
                int docId;
                if (int.TryParse(Request.QueryString["id"], out docId))
                {
                    //send notifications! TODO: This should be put somewhere centralized instead of hard coded directly here
                    Services.NotificationService.SendNotification(
                        Services.ContentService.GetById(docId), ActionToPublish.Instance);
                }

            }

        }


    }
}
