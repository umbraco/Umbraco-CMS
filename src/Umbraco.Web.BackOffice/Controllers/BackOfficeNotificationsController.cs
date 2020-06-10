using Microsoft.AspNetCore.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// An abstract controller that automatically checks if any request is a non-GET and if the
    /// resulting message is INotificationModel in which case it will append any Event Messages
    /// currently in the request.
    /// </summary>
    [TypeFilter(typeof(AppendCurrentEventMessagesAttribute))]
    public abstract class BackOfficeNotificationsController : UmbracoAuthorizedJsonController
    {

    }
}
