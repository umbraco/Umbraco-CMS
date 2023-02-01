using Umbraco.Cms.Core.Persistence;

namespace Umbraco.Cms.Infrastructure.Persistence.EfCore;

public class UmbracoEfCoreDatabase : IUmbracoEfCoreDatabase
{
    private readonly IDatabaseInfo _databaseInfo;
    private readonly Guid _instanceGuid = Guid.NewGuid();
    private string? _instanceId;

    public UmbracoEfCoreDatabase(IDatabaseInfo databaseInfo)
    {
        _databaseInfo = databaseInfo;
        DbContext = new UmbracoDbContext();
    }

    public UmbracoDbContext DbContext { get; }

    public string InstanceId => _instanceId ??= _instanceGuid.ToString("N").Substring(0, 8);

    public bool InTransaction => DbContext.Database.CurrentTransaction is not null;

    public async Task<bool> IsUmbracoInstalled() => await _databaseInfo.IsUmbracoInstalledAsync();
}
