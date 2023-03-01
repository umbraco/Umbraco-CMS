using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Services;

namespace Umbraco.Cms.Persistence.EFCore;

internal class UmbracoEfCoreDatabase : IUmbracoEfCoreDatabase
{
    private readonly IDistributedLockingMechanismFactory _distributedLockingMechanismFactory;
    private readonly IDatabaseInfo _databaseInfo;
    private readonly Guid _instanceGuid = Guid.NewGuid();
    private string? _instanceId;

    public UmbracoEfCoreDatabase(IDistributedLockingMechanismFactory distributedLockingMechanismFactory, IDatabaseInfo databaseInfo, UmbracoEFContext umbracoEfContext)
    {
        _distributedLockingMechanismFactory = distributedLockingMechanismFactory;
        _databaseInfo = databaseInfo;
        UmbracoEFContext = umbracoEfContext;
        Locks = new LockingMechanism(_distributedLockingMechanismFactory);
    }

    public ILockingMechanism Locks { get; }

    public UmbracoEFContext UmbracoEFContext { get; }

    public string InstanceId => _instanceId ??= _instanceGuid.ToString("N").Substring(0, 8);

    public bool InTransaction => UmbracoEFContext.Database.CurrentTransaction is not null;

    public async Task<bool> IsUmbracoInstalled() => await _databaseInfo.IsUmbracoInstalledAsync();

    public void Dispose() => UmbracoEFContext.Dispose();
}
