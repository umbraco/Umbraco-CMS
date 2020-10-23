using Umbraco.Web.BackOffice.Filters;

namespace Umbraco.Web.BackOffice.Controllers
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
    }
}
