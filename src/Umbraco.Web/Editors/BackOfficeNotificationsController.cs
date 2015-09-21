using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An abstract controller that automatically checks if any request is a non-GET and if the 
    /// resulting message is INotificationModel in which case it will append any Event Messages
    /// currently in the request.
    /// </summary>
    [AppendCurrentEventMessages]
    public abstract class BackOfficeNotificationsController : UmbracoAuthorizedJsonController
    {
        protected BackOfficeNotificationsController()
        {
        }

        protected BackOfficeNotificationsController(UmbracoContext umbracoContext) : base(umbracoContext)
        {
        }
    }
}