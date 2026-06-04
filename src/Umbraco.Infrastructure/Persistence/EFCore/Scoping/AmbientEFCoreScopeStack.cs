using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Scoping;
using CoreEFCoreScopeAccessor = Umbraco.Cms.Core.Scoping.EFCore.IScopeAccessor;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

/// <summary>
/// Thread-safe stack implementation for managing ambient EF Core scopes using AsyncLocal storage.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
public class AmbientEFCoreScopeStack<TDbContext> : IAmbientEFCoreScopeStack<TDbContext> where TDbContext : DbContext
{
    private static Lock _lock = new();
    private static AsyncLocal<ConcurrentStack<IEFCoreScope<TDbContext>>> _stack = new();

    /// <inheritdoc />
    public IEFCoreScope<TDbContext>? AmbientScope
    {
        get
        {
            lock (_lock)
            {
                if (_stack.Value?.TryPeek(out IEFCoreScope<TDbContext>? ambientScope) ?? false)
                {
                    return ambientScope;
                }

                return null;
            }
        }
    }

    /// <inheritdoc />
    ICoreScope? CoreEFCoreScopeAccessor.AmbientScope => AmbientScope;

    /// <inheritdoc />
    public IEFCoreScope<TDbContext> Pop()
    {
        lock (_lock)
        {
            if (_stack.Value?.TryPop(out IEFCoreScope<TDbContext>? ambientScope) ?? false)
            {
                return ambientScope;
            }

            throw new InvalidOperationException("No AmbientScope was found.");
        }
    }

    /// <inheritdoc />
    public void Push(IEFCoreScope<TDbContext> scope)
    {
        lock (_lock)
        {
            _stack.Value ??= new ConcurrentStack<IEFCoreScope<TDbContext>>();

            _stack.Value.Push(scope);
        }
    }
}
