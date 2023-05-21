using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public class AmbientEFCoreScopeStack<TDbContext> : IAmbientEFCoreScopeStack<TDbContext> where TDbContext : DbContext
{

    private static AsyncLocal<ConcurrentStack<IEfCoreScope<TDbContext>>> _stack = new();

    public IEfCoreScope<TDbContext>? AmbientScope
    {
        get
        {
            if (_stack.Value?.TryPeek(out IEfCoreScope<TDbContext>? ambientScope) ?? false)
            {
                return ambientScope;
            }

            return null;
        }
    }

    public IEfCoreScope<TDbContext> Pop()
    {
        if (_stack.Value?.TryPop(out IEfCoreScope<TDbContext>? ambientScope) ?? false)
        {
            return ambientScope;
        }

        throw new InvalidOperationException("No AmbientScope was found.");
    }

    public void Push(IEfCoreScope<TDbContext> scope)
    {
        _stack.Value ??= new ConcurrentStack<IEfCoreScope<TDbContext>>();

        _stack.Value.Push(scope);
    }
}
