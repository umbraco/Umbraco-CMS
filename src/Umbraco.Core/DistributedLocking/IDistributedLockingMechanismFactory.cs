namespace Umbraco.Cms.Core.DistributedLocking;

/// <summary>
///     Picks an appropriate IDistributedLockingMechanism when multiple are registered
/// </summary>
public interface IDistributedLockingMechanismFactory
{
    /// <summary>
    ///     Gets the most appropriate distributed locking mechanism for the current environment.
    /// </summary>
    IDistributedLockingMechanism DistributedLockingMechanism { get; }
}
