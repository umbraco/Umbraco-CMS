using Microsoft.AspNetCore.Mvc;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Common.ActionResults
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
