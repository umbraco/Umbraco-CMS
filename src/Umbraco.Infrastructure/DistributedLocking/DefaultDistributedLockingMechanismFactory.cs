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
    private IDistributedLockingMechanism _distributedLockingMechanism = null!;
    private bool _initialized;
    private object _lock = new();

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
            EnsureInitialized();

            return _distributedLockingMechanism;
        }
    }

    private void EnsureInitialized()
        => LazyInitializer.EnsureInitialized(ref _distributedLockingMechanism, ref _initialized, ref _lock, Initialize);

    private IDistributedLockingMechanism Initialize()
    {
        var configured = _globalSettings.CurrentValue.DistributedLockingMechanism;

        if (!string.IsNullOrEmpty(configured))
        {
            IDistributedLockingMechanism? value = _distributedLockingMechanisms
                .FirstOrDefault(x => x.GetType().FullName?.EndsWith(configured) ?? false);

            if (value == null)
            {
                throw new InvalidOperationException(
                    $"Couldn't find DistributedLockingMechanism specified by global config: {configured}");
            }
        }

        IDistributedLockingMechanism? defaultMechanism = _distributedLockingMechanisms.FirstOrDefault(x => x.Enabled);
        if (defaultMechanism != null)
        {
            return defaultMechanism;
        }

        throw new InvalidOperationException("Couldn't find an appropriate default distributed locking mechanism.");
    }
}
