using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations;

public class ClearCsrfCookieHandler : INotificationHandler<UmbracoPlanExecutedNotification>
{
    private readonly ICookieManager _cookieManager;

    public ClearCsrfCookieHandler(ICookieManager cookieManager)
    {
        _cookieManager = cookieManager;
    }

    public void Handle(UmbracoPlanExecutedNotification notification)
    {
        // We'll only clear the cookie if the migration actually succeeded.
        if (notification.ExecutedPlan.Successful is false)
        {
            return;
        }

        _cookieManager.ExpireCookie(Constants.Web.CsrfValidationCookieName);
    }
}
