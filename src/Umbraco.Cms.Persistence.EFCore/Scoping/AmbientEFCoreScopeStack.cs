using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

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
            ConcurrentStack<IEFCoreScope<TDbContext>>? stack = _stack.Value;

            // An empty stack was inherited from an ancestor execution context, because popping the last entry
            // leaves the instance in place. AsyncLocal copy-on-write protects the reference and not the instance
            // it points at, so pushing onto it would share this flow's scopes with every sibling flow that
            // inherited it. A non-empty stack is genuine nesting within this flow and is kept.
            if (stack is null || stack.IsEmpty)
            {
                stack = new ConcurrentStack<IEFCoreScope<TDbContext>>();
                _stack.Value = stack;
            }

            stack.Push(scope);
        }
    }
}
