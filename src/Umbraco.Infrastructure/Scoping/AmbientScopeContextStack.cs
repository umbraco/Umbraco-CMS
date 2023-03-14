using System.Collections.Concurrent;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Infrastructure.Scoping;

internal class AmbientScopeContextStack : IAmbientScopeContextStack
{
    private static AsyncLocal<ConcurrentStack<IScopeContext>> _stack = new();

    public IScopeContext? AmbientContext
    {
        get
        {
            if (_stack.Value?.TryPeek(out IScopeContext? ambientContext) ?? false)
            {
                return ambientContext;
            }

            return null;
        }
    }

    public IScopeContext Pop()
    {
        if (_stack.Value?.TryPop(out IScopeContext? ambientContext) ?? false)
        {
            return ambientContext;
        }

        throw new InvalidOperationException("No AmbientContext was found.");
    }

    public void Push(IScopeContext scope)
    {
        _stack.Value ??= new ConcurrentStack<IScopeContext>();

        _stack.Value.Push(scope);
    }
}
