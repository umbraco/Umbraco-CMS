using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
///     Recurring hosted service that executes the content history cleanup.
/// </summary>
public class ContentVersionCleanup : RecurringHostedServiceBase
{
    private readonly ILogger<ContentVersionCleanup> _logger;
    private readonly IMainDom _mainDom;
    private readonly IRuntimeState _runtimeState;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly IContentVersionService _service;
    private readonly IOptionsMonitor<ContentSettings> _settingsMonitor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentVersionCleanup" /> class.
    /// </summary>
    public ContentVersionCleanup(
        IRuntimeState runtimeState,
        ILogger<ContentVersionCleanup> logger,
        IOptionsMonitor<ContentSettings> settingsMonitor,
        IContentVersionService service,
        IMainDom mainDom,
        IServerRoleAccessor serverRoleAccessor)
        : base(logger, TimeSpan.FromHours(1), TimeSpan.FromMinutes(3))
    {
        _runtimeState = runtimeState;
        _logger = logger;
        _settingsMonitor = settingsMonitor;
        _service = service;
        _mainDom = mainDom;
        _serverRoleAccessor = serverRoleAccessor;
    }

    /// <inheritdoc />
    public override Task PerformExecuteAsync(object? state)
    {
        // Globally disabled by feature flag
        if (!_settingsMonitor.CurrentValue.ContentVersionCleanupPolicy.EnableCleanup)
        {
            _logger.LogInformation(
                "ContentVersionCleanup task will not run as it has been globally disabled via configuration");
            return Task.CompletedTask;
        }

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

        var count = _service.PerformContentVersionCleanup(DateTime.Now).Count;

        if (count > 0)
        {
            _logger.LogInformation("Deleted {count} ContentVersion(s)", count);
        }
        else
        {
            _logger.LogDebug("Task complete, no items were Deleted");
        }

        return Task.FromResult(true);
    }
}
