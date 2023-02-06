using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScope : IEfCoreScope
{
    private readonly IUmbracoEfCoreDatabaseFactory _efCoreDatabaseFactory;
    private IUmbracoEfCoreDatabase? _umbracoEfCoreDatabase;
    public bool? _completed;

    public EfCoreScope(IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory)
    {
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
    }

    public IUmbracoEfCoreDatabase UmbracoEfCoreDatabase
    {
        get
        {
            if (_umbracoEfCoreDatabase is not null)
            {
                return _umbracoEfCoreDatabase;
            }

            _umbracoEfCoreDatabase = _efCoreDatabaseFactory.Create();
            _umbracoEfCoreDatabase.UmbracoEFContext.Database.BeginTransaction();
            return _umbracoEfCoreDatabase;
        }
    }

    public void Complete() => _completed = true;

    public void Dispose()
    {
        DisposeEfCoreDatabase();
    }

    private void DisposeEfCoreDatabase()
    {
        var completed = _completed.HasValue && _completed.Value;
        if (_umbracoEfCoreDatabase is not null)
        {
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
                _umbracoEfCoreDatabase = null;
            }
        }
    }
}
