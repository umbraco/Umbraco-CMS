using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Install;

/// <summary>
/// Coordinates migration leadership across servers in a load-balanced environment.
/// Exactly one server claims leadership, runs all migrations, then releases the claim.
/// All other servers wait until the leader finishes, then proceed with per-server initialization.
/// </summary>
internal sealed class MigrationCoordinator : IMigrationCoordinator
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IKeyValueService _keyValueService;
    private readonly IRuntimeState _runtimeState;
    private readonly IMachineInfoFactory _machineInfoFactory;
    private readonly IOptions<UnattendedSettings> _unattendedSettings;
    private readonly ILogger<MigrationCoordinator> _logger;
    private string? _leaderClaim;

    public MigrationCoordinator(
        ICoreScopeProvider scopeProvider,
        IKeyValueService keyValueService,
        IRuntimeState runtimeState,
        IMachineInfoFactory machineInfoFactory,
        IOptions<UnattendedSettings> unattendedSettings,
        ILogger<MigrationCoordinator> logger)
    {
        _scopeProvider = scopeProvider;
        _keyValueService = keyValueService;
        _runtimeState = runtimeState;
        _machineInfoFactory = machineInfoFactory;
        _unattendedSettings = unattendedSettings;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> TryBecomeLeaderAsync(CancellationToken cancellationToken)
    {
        var machineIdentifier = _machineInfoFactory.GetMachineIdentifier();

        while (cancellationToken.IsCancellationRequested is false)
        {
            if (TryClaimLeadership(machineIdentifier))
            {
                _logger.LogInformation("This server claimed migration leadership.");
                return true;
            }

            try
            {
                _runtimeState.DetermineRuntimeLevel();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not determine runtime level during migration wait; will retry.");
            }

            switch (_runtimeState.Level)
            {
                case RuntimeLevel.Run:
                    _logger.LogInformation("Migrations completed by another server; proceeding as follower.");
                    return false;
                case RuntimeLevel.BootFailed:
                    _logger.LogError("Runtime entered BootFailed state while waiting for migrations.");
                    return false;
                default:
                    _logger.LogDebug("Waiting for migration leader to finish...");
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    break;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public void ReleaseLeadership()
    {
        if (_leaderClaim is null)
        {
            return;
        }

        try
        {
            using ICoreScope scope = _scopeProvider.CreateCoreScope();
            scope.WriteLock(Constants.Locks.KeyValues);

            string? current = _keyValueService.GetValue(Constants.Conventions.Migrations.UpgradeLockKey);
            if (current == _leaderClaim)
            {
                _keyValueService.SetValue(Constants.Conventions.Migrations.UpgradeLockKey, string.Empty);
            }

            scope.Complete();
            _leaderClaim = null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to release migration leadership; continuing shutdown because leadership release is best-effort.");
        }
    }

    private static bool IsStale(string claim, TimeSpan timeout)
    {
        var separatorIndex = claim.IndexOf('|');
        return separatorIndex < 0
               || !DateTimeOffset.TryParse(claim.AsSpan(separatorIndex + 1), out DateTimeOffset timestamp)
               || DateTimeOffset.UtcNow - timestamp > timeout;
    }

    // Acquires WriteLock(KeyValues) so the read-then-write is serialized across all servers.
    // Inner GetValue and SetValue calls create nested scopes that join the outer transaction;
    // their internal WriteLock requests are no-ops because the lock is already held.
    private bool TryClaimLeadership(string machineIdentifier)
    {
        TimeSpan timeout = _unattendedSettings.Value.MigrationClaimTimeout;

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.KeyValues);

        string? current = _keyValueService.GetValue(Constants.Conventions.Migrations.UpgradeLockKey);

        bool canClaim = string.IsNullOrEmpty(current)
            || IsStale(current, timeout)
            || current.StartsWith(machineIdentifier + "|", StringComparison.Ordinal);

        if (canClaim)
        {
            _leaderClaim = $"{machineIdentifier}|{DateTimeOffset.UtcNow:O}";
            _keyValueService.SetValue(Constants.Conventions.Migrations.UpgradeLockKey, _leaderClaim);
        }

        scope.Complete();
        return canClaim;
    }
}
