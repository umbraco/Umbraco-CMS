using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Common.AspNetCore;

/// <summary>
/// Notification handler which will listen to the <see cref="UmbracoRequestBeginNotification"/>, and ensure that
/// the applicationUrl is set on the first request.
/// </summary>
internal class ApplicationUrlRequestBeginNotificationHandler : INotificationHandler<UmbracoRequestBeginNotification>
{
    private readonly IRequestAccessor _requestAccessor;

    public ApplicationUrlRequestBeginNotificationHandler(IRequestAccessor requestAccessor) =>
        _requestAccessor = requestAccessor;

    public void Handle(UmbracoRequestBeginNotification notification)
    {
        // If someone has replaced the AspNetCoreRequestAccessor we'll do nothing and assume they handle it themselves.
        if (_requestAccessor is AspNetCoreRequestAccessor accessor)
        {
            accessor.EnsureApplicationUrl();
        }
    }
}
