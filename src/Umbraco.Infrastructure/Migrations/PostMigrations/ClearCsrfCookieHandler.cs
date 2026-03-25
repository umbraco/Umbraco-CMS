using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations;

/// <summary>
/// Provides functionality to clear CSRF cookies during post-migration operations.
/// </summary>
public class ClearCsrfCookieHandler : INotificationHandler<UmbracoPlanExecutedNotification>
{
    private readonly ICookieManager _cookieManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearCsrfCookieHandler"/> class.
    /// </summary>
    /// <param name="cookieManager">The <see cref="ICookieManager"/> used to manage and clear the CSRF cookie.</param>
    public ClearCsrfCookieHandler(ICookieManager cookieManager)
    {
        _cookieManager = cookieManager;
    }

    /// <summary>
    /// Handles the <see cref="UmbracoPlanExecutedNotification"/> by clearing the CSRF validation cookie if the executed migration plan was successful.
    /// This helps ensure that any potentially stale CSRF tokens are removed after a successful migration.
    /// </summary>
    /// <param name="notification">The notification containing details about the executed migration plan.</param>
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
