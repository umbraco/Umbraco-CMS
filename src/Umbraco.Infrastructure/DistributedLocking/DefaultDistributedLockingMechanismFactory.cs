using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;

namespace Umbraco.Cms.Infrastructure.DistributedLocking;

public class DefaultDistributedLockingMechanismFactory : IDistributedLockingMechanismFactory
{
    private readonly IEnumerable<IDistributedLockingMechanism> _distributedLockingMechanisms;
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
    private IDistributedLockingMechanism? _configuredMechanism;
    private readonly object _lock = new();

    public DefaultDistributedLockingMechanismFactory(
        IOptionsMonitor<GlobalSettings> globalSettings,
        IEnumerable<IDistributedLockingMechanism> distributedLockingMechanisms)
    {
        _globalSettings = globalSettings;
        _distributedLockingMechanisms = distributedLockingMechanisms;
    }

    public IDistributedLockingMechanism DistributedLockingMechanism
    {
        get
        {
            // Fast path: configured mechanism is cached after first resolution.
            if (_configuredMechanism is not null)
            {
                return _configuredMechanism;
            }

            var configured = _globalSettings.CurrentValue.DistributedLockingMechanism;

            if (string.IsNullOrEmpty(configured) is false)
            {
                // Configured mechanism is a static admin choice — resolve and cache it once.
                lock (_lock)
                {
                    _configuredMechanism ??= _distributedLockingMechanisms
                        .FirstOrDefault(x => x.GetType().FullName?.EndsWith(configured) ?? false)
                        ?? throw new InvalidOperationException(
                            $"Couldn't find DistributedLockingMechanism specified by global config: {configured}");
                }

                return _configuredMechanism;
            }

            // Default mechanism is selected dynamically on every call because Enabled is
            // context-sensitive (e.g. SqliteEFCoreDistributedLockingMechanism.Enabled checks
            // whether an EF Core ambient scope is currently active).
            return _distributedLockingMechanisms.FirstOrDefault(x => x.Enabled)
                ?? throw new InvalidOperationException("Couldn't find an appropriate default distributed locking mechanism.");
        }
    }
}
