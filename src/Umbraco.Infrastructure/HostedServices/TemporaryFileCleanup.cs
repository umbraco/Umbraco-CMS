using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
///     Recurring hosted service that executes the temporary file cleanup.
/// </summary>
public class TemporaryFileCleanup : RecurringHostedServiceBase
{
    private readonly ILogger<TemporaryFileCleanup> _logger;
    private readonly IMainDom _mainDom;
    private readonly IRuntimeState _runtimeState;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly ITemporaryFileService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemporaryFileCleanup" /> class.
    /// </summary>
    public TemporaryFileCleanup(
        IRuntimeState runtimeState,
        ILogger<TemporaryFileCleanup> logger,
        ITemporaryFileService temporaryFileService,
        IMainDom mainDom,
        IServerRoleAccessor serverRoleAccessor)
        : base(logger, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5))
    {
        _runtimeState = runtimeState;
        _logger = logger;
        _service = temporaryFileService;
        _mainDom = mainDom;
        _serverRoleAccessor = serverRoleAccessor;
    }

    /// <inheritdoc />
    public override Task PerformExecuteAsync(object? state)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return Task.FromResult(true); // repeat...
        }

        // Don't run on replicas nor unknown role servers
        switch (_serverRoleAccessor.CurrentServerRole)
        {
            case ServerRole.Subscriber:
                _logger.LogDebug("Does not run on subscriber servers");
                return Task.CompletedTask;
            case ServerRole.Unknown:
                _logger.LogDebug("Does not run on servers with unknown role");
                return Task.CompletedTask;
            case ServerRole.Single:
            case ServerRole.SchedulingPublisher:
            default:
                break;
        }

        // Ensure we do not run if not main domain, but do NOT lock it
        if (!_mainDom.IsMainDom)
        {
            _logger.LogDebug("Does not run if not MainDom");
            return Task.FromResult(false); // do NOT repeat, going down
        }

        var count = _service.CleanUpOldTempFiles().GetAwaiter().GetResult().Count();

        if (count > 0)
        {
            _logger.LogDebug("Deleted {Count} temporary file(s)", count);
        }
        else
        {
            _logger.LogDebug("Task complete, no items were deleted");
        }

        return Task.FromResult(true);
    }
}
