using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Scoping;
using CoreEFCoreScopeAccessor = Umbraco.Cms.Core.Scoping.EFCore.IScopeAccessor;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

/// <summary>
/// Thread-safe stack implementation for managing ambient EF Core scopes using AsyncLocal storage.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
public class AmbientEFCoreScopeStack<TDbContext> : IAmbientEFCoreScopeStack<TDbContext> where TDbContext : DbContext
{
    private static Lock _lock = new();
    private static AsyncLocal<ConcurrentStack<IEfCoreScope<TDbContext>>> _stack = new();

    /// <inheritdoc />
    public IEfCoreScope<TDbContext>? AmbientScope
    {
        get
        {
            lock (_lock)
            {
                if (_stack.Value?.TryPeek(out IEfCoreScope<TDbContext>? ambientScope) ?? false)
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
    public IEfCoreScope<TDbContext> Pop()
    {
        lock (_lock)
        {
            if (_stack.Value?.TryPop(out IEfCoreScope<TDbContext>? ambientScope) ?? false)
            {
                return ambientScope;
            }

            throw new InvalidOperationException("No AmbientScope was found.");
        }
    }

    /// <inheritdoc />
    public void Push(IEfCoreScope<TDbContext> scope)
    {
        lock (_lock)
        {
            _stack.Value ??= new ConcurrentStack<IEfCoreScope<TDbContext>>();

            _stack.Value.Push(scope);
        }
    }
}
