using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Web.UI;

public class MyNotificationHandler : INotificationHandler<ContentSavedNotification>
{
    private readonly IScopeProvider _scopeProvider;

    public MyNotificationHandler(IScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }
    public void Handle(ContentSavedNotification notification)
    {
        using (var scope = _scopeProvider.CreateScope())
        {
            // scope.Database.
        }
    }
}
