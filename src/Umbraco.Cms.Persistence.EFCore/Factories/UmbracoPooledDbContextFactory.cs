using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Persistence.EFCore.Factories;

/// <inheritdoc/>
internal class UmbracoPooledDbContextFactory<TContext> : PooledDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly IRuntimeState _runtimeState;
    private readonly DbContextOptions<TContext> _options;

    /// <inheritdoc/>
    public UmbracoPooledDbContextFactory(IRuntimeState runtimeState, DbContextOptions<TContext> options, int poolSize = 1024 /*DbContextPool<DbContext>.DefaultPoolSize*/) : base(options, poolSize)
    {
        _runtimeState = runtimeState;
        _options = options;
    }

    /// <inheritdoc/>
    public override TContext CreateDbContext()
    {
        if (_runtimeState.Level == RuntimeLevel.Run)
        {
            return base.CreateDbContext();
        }
        else
        {
            return (TContext?)Activator.CreateInstance(typeof(TContext), _options) ?? throw new InvalidOperationException("Unable to create DbContext");
        }
    }

    /// <inheritdoc/>
    public override async Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        if (_runtimeState.Level == RuntimeLevel.Run)
        {
            return await base.CreateDbContextAsync(cancellationToken);
        }
        else
        {
            return (TContext?)Activator.CreateInstance(typeof(TContext), _options) ?? throw new InvalidOperationException("Unable to create DbContext");
        }
    }
}
