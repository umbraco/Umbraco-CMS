using System.Collections.Concurrent;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public class AmbientEfCoreScopeStack : IAmbientEfCoreScopeStack
{

    private static AsyncLocal<ConcurrentStack<IEfCoreScope>> _stack = new();

    public IEfCoreScope? AmbientScope { get; }

    public IEfCoreScope Pop()
    {
        if (_stack.Value?.TryPop(out IEfCoreScope? ambientScope) ?? false)
        {
            return ambientScope;
        }

        throw new InvalidOperationException("No AmbientScope was found.");
    }

    public void Push(IEfCoreScope scope)
    {
        _stack.Value ??= new ConcurrentStack<IEfCoreScope>();

        _stack.Value.Push(scope);
    }
}
