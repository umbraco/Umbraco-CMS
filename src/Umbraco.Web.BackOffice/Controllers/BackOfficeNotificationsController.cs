using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    /// <summary>
    /// An abstract controller that automatically checks if any request is a non-GET and if the
    /// resulting message is INotificationModel in which case it will append any Event Messages
    /// currently in the request.
    /// </summary>
    [PrefixlessBodyModelValidator]
    [AppendCurrentEventMessages]
    public abstract class BackOfficeNotificationsController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Overridden to ensure that the error message is an error notification message
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        protected override ActionResult ValidationProblem(string errorMessage)
        {
            var notificationModel = new SimpleNotificationModel
            {
                Message = errorMessage
            };
            notificationModel.AddErrorNotification(errorMessage, string.Empty);
            return new ValidationErrorResult(notificationModel);
        }

        /// <summary>
        /// Overridden to ensure that all queued notifications are sent to the back office
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public override ActionResult ValidationProblem()
            // returning an object of INotificationModel will ensure that any pending
            // notification messages are added to the response.
            => new ValidationErrorResult(new SimpleNotificationModel());
    
    }
}
