using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Web.BackOffice.ActionResults
{
    public class UmbracoNotificationSuccessResponse : OkObjectResult
    {
        public UmbracoNotificationSuccessResponse(string successMessage) : base(null)
        {
            var notificationModel = new SimpleNotificationModel
            {
                Message = successMessage
            };
            notificationModel.AddSuccessNotification(successMessage, string.Empty);

            Value = notificationModel;
        }
    }
}
