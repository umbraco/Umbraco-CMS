using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Infrastructure.Persistence.EfCore;

internal class UmbracoEfCoreDatabase : IUmbracoEfCoreDatabase
{
    private readonly IDatabaseInfo _databaseInfo;
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;
    private readonly Guid _instanceGuid = Guid.NewGuid();
    private string? _instanceId;

    public UmbracoEfCoreDatabase(IDatabaseInfo databaseInfo, UmbracoEFContext umbracoEfContext, UmbracoDbContextFactory umbracoDbContextFactory)
    {
        _databaseInfo = databaseInfo;
        _umbracoDbContextFactory = umbracoDbContextFactory;
        UmbracoEFContext = umbracoEfContext;
    }

    public Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method) => _umbracoDbContextFactory.ExecuteWithContextAsync<T>(method);

    public UmbracoEFContext UmbracoEFContext { get; }
    public string InstanceId => _instanceId ??= _instanceGuid.ToString("N").Substring(0, 8);

    public bool InTransaction => UmbracoEFContext.Database.CurrentTransaction is not null;

    public async Task<bool> IsUmbracoInstalled() => await _databaseInfo.IsUmbracoInstalledAsync();

    public void Dispose() => UmbracoEFContext.Dispose();
}
