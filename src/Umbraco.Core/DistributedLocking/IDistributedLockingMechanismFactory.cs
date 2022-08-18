namespace Umbraco.Cms.Core.DistributedLocking;

/// <summary>
///     Picks an appropriate IDistributedLockingMechanism when multiple are registered
/// </summary>
public interface IDistributedLockingMechanismFactory
{
    IDistributedLockingMechanism DistributedLockingMechanism { get; }
}
