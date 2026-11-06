using Microsoft.Extensions.Options;
using OpenIddict.Server;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
///     Clears the cached OpenIddict server options when the runtime restarts.
/// </summary>
/// <remarks>
///     The runtime restarts after an install, at which point the database becomes available, so the options must be
///     rebuilt for <see cref="ConfigureOpenIddictServerCredentials"/> to replace the ephemeral credentials used until then.
/// </remarks>
internal sealed class ResetOpenIddictServerOptionsHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly IOptionsMonitorCache<OpenIddictServerOptions> _optionsMonitorCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResetOpenIddictServerOptionsHandler"/> class.
    /// </summary>
    /// <param name="optionsMonitorCache">The cache of the OpenIddict server options.</param>
    public ResetOpenIddictServerOptionsHandler(IOptionsMonitorCache<OpenIddictServerOptions> optionsMonitorCache)
        => _optionsMonitorCache = optionsMonitorCache;

    /// <inheritdoc/>
    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        if (notification.IsRestarting)
        {
            _optionsMonitorCache.TryRemove(Options.DefaultName);
        }
    }
}
