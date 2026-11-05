// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Sync;

/// <inheritdoc cref="IServerRoleElector" />
internal sealed class ServerRoleElector : IServerRoleElector
{
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly IServerRegistrationService _serverRegistrationService;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
    private readonly ILogger<ServerRoleElector> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerRoleElector"/> class.
    /// </summary>
    public ServerRoleElector(
        IServerRoleAccessor serverRoleAccessor,
        IServerRegistrationService serverRegistrationService,
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<GlobalSettings> globalSettings,
        ILogger<ServerRoleElector> logger)
    {
        _serverRoleAccessor = serverRoleAccessor;
        _serverRegistrationService = serverRegistrationService;
        _hostingEnvironment = hostingEnvironment;
        _globalSettings = globalSettings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task TryElectOnceAsync(CancellationToken cancellationToken)
    {
        if (_serverRoleAccessor is not ElectedServerRoleAccessor)
        {
            return;
        }

        _ = await BoundedServerTouch.TryTouchAsync(
            _serverRegistrationService,
            _hostingEnvironment,
            _globalSettings.CurrentValue,
            _logger,
            cancellationToken);
    }
}
