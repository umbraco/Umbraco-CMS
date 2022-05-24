using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     An abstract controller that automatically checks if any request is a non-GET and if the
///     resulting message is INotificationModel in which case it will append any Event Messages
///     currently in the request.
/// </summary>
[PrefixlessBodyModelValidator]
[AppendCurrentEventMessages]
public abstract class BackOfficeNotificationsController : UmbracoAuthorizedJsonController
{
    /// <summary>
    ///     returns a 200 OK response with a notification message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected OkObjectResult Ok(string message)
    {
        var notificationModel = new SimpleNotificationModel { Message = message };
        notificationModel.AddSuccessNotification(message, string.Empty);

        return new OkObjectResult(notificationModel);
    }

    /// <summary>
    ///     Overridden to ensure that the error message is an error notification message
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    protected override ActionResult ValidationProblem(string? errorMessage)
        => ValidationProblem(errorMessage, string.Empty);

    /// <summary>
    ///     Creates a notofication validation problem with a header and message
    /// </summary>
    /// <param name="errorHeader"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    protected ActionResult ValidationProblem(string? errorHeader, string errorMessage)
    {
        var notificationModel = new SimpleNotificationModel { Message = errorMessage };
        notificationModel.AddErrorNotification(errorHeader, errorMessage);
        return new ValidationErrorResult(notificationModel);
    }

    /// <summary>
    ///     Overridden to ensure that all queued notifications are sent to the back office
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public override ActionResult ValidationProblem()
        // returning an object of INotificationModel will ensure that any pending
        // notification messages are added to the response.
        => new ValidationErrorResult(new SimpleNotificationModel());
}
