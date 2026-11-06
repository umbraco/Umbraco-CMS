using Microsoft.Extensions.Options;
using OpenIddict.Server;
using OpenIddict.Validation;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
///     Clears the cached OpenIddict server and validation options when the runtime restarts.
/// </summary>
/// <remarks>
///     The runtime restarts after an install, at which point the database becomes available, so the options must be
///     rebuilt for <see cref="ConfigureOpenIddictServerCredentials"/> to replace the ephemeral credentials used until then.
///     The validation options copy the server credentials, so they are rebuilt too.
/// </remarks>
internal sealed class ResetOpenIddictServerOptionsHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly IOptionsMonitorCache<OpenIddictServerOptions> _serverOptionsCache;
    private readonly IOptionsMonitorCache<OpenIddictValidationOptions> _validationOptionsCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResetOpenIddictServerOptionsHandler"/> class.
    /// </summary>
    /// <param name="serverOptionsCache">The cache of the OpenIddict server options.</param>
    /// <param name="validationOptionsCache">The cache of the OpenIddict validation options.</param>
    public ResetOpenIddictServerOptionsHandler(
        IOptionsMonitorCache<OpenIddictServerOptions> serverOptionsCache,
        IOptionsMonitorCache<OpenIddictValidationOptions> validationOptionsCache)
    {
        _serverOptionsCache = serverOptionsCache;
        _validationOptionsCache = validationOptionsCache;
    }

    /// <inheritdoc/>
    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        if (notification.IsRestarting)
        {
            _serverOptionsCache.TryRemove(Options.DefaultName);
            _validationOptionsCache.TryRemove(Options.DefaultName);
        }
    }
}
