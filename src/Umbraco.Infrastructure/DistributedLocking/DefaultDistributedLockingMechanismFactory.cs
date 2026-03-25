using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;

namespace Umbraco.Cms.Infrastructure.DistributedLocking;

/// <summary>
/// Represents the default factory for creating distributed locking mechanisms in Umbraco.
/// </summary>
public class DefaultDistributedLockingMechanismFactory : IDistributedLockingMechanismFactory
{
    private readonly IEnumerable<IDistributedLockingMechanism> _distributedLockingMechanisms;
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
    private IDistributedLockingMechanism? _configuredMechanism;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultDistributedLockingMechanismFactory"/> class.
    /// </summary>
    /// <param name="globalSettings">An <see cref="IOptionsMonitor{T}"/> for accessing <see cref="GlobalSettings"/> at runtime.</param>
    /// <param name="distributedLockingMechanisms">A collection of available <see cref="IDistributedLockingMechanism"/> implementations to be used by the factory.</param>
    public DefaultDistributedLockingMechanismFactory(
        IOptionsMonitor<GlobalSettings> globalSettings,
        IEnumerable<IDistributedLockingMechanism> distributedLockingMechanisms)
    {
        _globalSettings = globalSettings;
        _distributedLockingMechanisms = distributedLockingMechanisms;
    }

    /// <summary>
    /// Gets the current instance of the distributed locking mechanism, ensuring it is initialized.
    /// </summary>
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
