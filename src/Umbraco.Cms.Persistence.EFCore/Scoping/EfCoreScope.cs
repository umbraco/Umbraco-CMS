using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScope : IEfCoreScope
{
    private readonly IUmbracoEfCoreDatabaseFactory _efCoreDatabaseFactory;
    private IUmbracoEfCoreDatabase _umbracoEfCoreDatabase;
    public bool? _completed;

    public EfCoreScope(IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory)
    {
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
        _umbracoEfCoreDatabase = _efCoreDatabaseFactory.Create();
        _umbracoEfCoreDatabase.UmbracoEFContext.Database.BeginTransaction();
    }

    public async Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method) => await method(_umbracoEfCoreDatabase.UmbracoEFContext);

    public void Complete() => _completed = true;

    public void Dispose()
    {
        DisposeEfCoreDatabase();
    }

    private void DisposeEfCoreDatabase()
    {
        var completed = _completed.HasValue && _completed.Value;
        try
        {
            if (completed)
            {
                _umbracoEfCoreDatabase.UmbracoEFContext.Database.CommitTransaction();
            }
            else
            {
                _umbracoEfCoreDatabase.UmbracoEFContext.Database.RollbackTransaction();
            }
        }
        finally
        {
            _efCoreDatabaseFactory.Dispose();
            _umbracoEfCoreDatabase.Dispose();
        }
    }
}
