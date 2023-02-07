using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScope : IEfCoreScope
{
    private readonly IUmbracoEfCoreDatabaseFactory _efCoreDatabaseFactory;
    private readonly IEFCoreScopeAccessor _efCoreScopeAccessor;
    private readonly IEfCoreScopeProvider _efCoreScopeProvider;
    private IUmbracoEfCoreDatabase? _umbracoEfCoreDatabase;
    private bool? _completed;
    private bool _disposed;

    public Guid InstanceId { get; }

    public EfCoreScope? ParentScope { get; }


    public EfCoreScope(
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        IEfCoreScopeProvider efCoreScopeProvider)
    {
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = efCoreScopeProvider;

        InstanceId = Guid.NewGuid();
    }

    public EfCoreScope(
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        IEfCoreScopeProvider efCoreScopeProvider,
        EfCoreScope parentScope)
        : this(
            efCoreDatabaseFactory,
            efCoreScopeAccessor,
            efCoreScopeProvider) =>
        ParentScope = parentScope;

    public async Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method)
    {
        if (_disposed)
        {
            throw new InvalidOperationException("The scope has been disposed, therefore the database is not available.");
        }

        if (_umbracoEfCoreDatabase is null)
        {
            InitializeDatabase();
        }

        return await method(_umbracoEfCoreDatabase!.UmbracoEFContext);
    }

    public async Task ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task> method) =>
        await ExecuteWithContextAsync(async db =>
        {
            await method(db);
            return true; // Do nothing
        });

    public void Complete()
    {
        if (_completed.HasValue == false)
        {
            _completed = true;
        }
    }

    public void Reset() => _completed = null;

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
        else
        {
            ParentScope.ChildCompleted(_completed);
        }

        _efCoreScopeProvider.PopAmbientScope();
        _disposed = true;
    }

    public void ChildCompleted(bool? completed)
    {
        // if child did not complete we cannot complete
        if (completed.HasValue == false || completed.Value == false)
        {
            _completed = false;
        }
    }

    private void InitializeDatabase()
    {
        _umbracoEfCoreDatabase = _efCoreDatabaseFactory.Create();

        // Check if we are already in a transaction before starting one
        if (_umbracoEfCoreDatabase.UmbracoEFContext.Database.CurrentTransaction is null)
        {
            _umbracoEfCoreDatabase.UmbracoEFContext.Database.BeginTransaction();
        }
    }

    private void DisposeEfCoreDatabase()
    {
        var completed = _completed.HasValue && _completed.Value;
        if (_umbracoEfCoreDatabase is not null)
        {
            if (completed)
            {
                _umbracoEfCoreDatabase.UmbracoEFContext.Database.CommitTransaction();
            }
            else
            {
                _umbracoEfCoreDatabase.UmbracoEFContext.Database.RollbackTransaction();
            }

            _umbracoEfCoreDatabase.Dispose();
        }

        _efCoreDatabaseFactory.Dispose();

    }
}
