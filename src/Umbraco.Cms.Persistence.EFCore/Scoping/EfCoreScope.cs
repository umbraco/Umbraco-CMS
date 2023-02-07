using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScope : IEfCoreScope
{
    private readonly IUmbracoEfCoreDatabaseFactory _efCoreDatabaseFactory;
    private readonly IEFCoreScopeAccessor _efCoreScopeAccessor;
    private readonly IEfCoreScopeProvider _efCoreScopeProvider;
    private readonly IUmbracoEfCoreDatabase _umbracoEfCoreDatabase;

    public Guid InstanceId { get; }

    public IEfCoreScope? ParentScope { get; }

    public bool? _completed;

    public EfCoreScope(IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory, IEFCoreScopeAccessor efCoreScopeAccessor, IEfCoreScopeProvider efCoreScopeProvider)
    {
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = efCoreScopeProvider;
        _umbracoEfCoreDatabase = _efCoreDatabaseFactory.Create();

        // Check if we are already in a transaction before starting one
        if (_umbracoEfCoreDatabase.UmbracoEFContext.Database.CurrentTransaction is null)
        {
            _umbracoEfCoreDatabase.UmbracoEFContext.Database.BeginTransaction();
        }

        InstanceId = Guid.NewGuid();
    }

    public EfCoreScope(
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        IEfCoreScopeProvider efCoreScopeProvider,
        IEfCoreScope parentScope)
        : this(
        efCoreDatabaseFactory,
        efCoreScopeAccessor,
        efCoreScopeProvider) =>
        ParentScope = parentScope;

    public async Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method) => await method(_umbracoEfCoreDatabase.UmbracoEFContext);

    public void Complete() => _completed = true;

    public void Dispose()
    {
        if (this != _efCoreScopeAccessor.AmbientScope)
        {
            var failedMessage =
                $"The {nameof(EfCoreScope)} {InstanceId} being disposed is not the Ambient {nameof(EfCoreScope)} {_efCoreScopeAccessor.AmbientScope?.InstanceId.ToString() ?? "NULL"}. This typically indicates that a child {nameof(EfCoreScope)} was not disposed, or flowed to a child thread that was not awaited, or concurrent threads are accessing the same {nameof(EfCoreScope)} (Ambient context) which is not supported. If using Task.Run (or similar) as a fire and forget tasks or to run threads in parallel you must suppress execution context flow with ExecutionContext.SuppressFlow() and ExecutionContext.RestoreFlow().";
            throw new InvalidOperationException(failedMessage);
        }

        if (ParentScope is null)
        {
            DisposeEfCoreDatabase();
        }

        _efCoreScopeProvider.PopAmbientScope();
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
