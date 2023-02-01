using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Infrastructure.Persistence.EfCore;

public class UmbracoEfCoreDatabase : IUmbracoEfCoreDatabase
{
    private readonly IDatabaseInfo _databaseInfo;
    private readonly Guid _instanceGuid = Guid.NewGuid();
    private string? _instanceId;

    public UmbracoEfCoreDatabase(IDatabaseInfo databaseInfo, UmbracoEFContext umbracoEfContext)
    {
        _databaseInfo = databaseInfo;
        DbContext = umbracoEfContext;
    }

    public DbContext DbContext { get; }

    public string InstanceId => _instanceId ??= _instanceGuid.ToString("N").Substring(0, 8);

    public bool InTransaction => DbContext.Database.CurrentTransaction is not null;

    public async Task<bool> IsUmbracoInstalled() => await _databaseInfo.IsUmbracoInstalledAsync();

    public void Dispose() => DbContext.Dispose();
}
