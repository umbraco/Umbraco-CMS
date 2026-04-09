using System.Collections.Concurrent;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Infrastructure.Scoping;

internal sealed class AmbientScopeContextStack : IAmbientScopeContextStack
{
    private static Lock _lock = new();
    private static AsyncLocal<ConcurrentStack<IScopeContext>> _stack = new();

    /// <summary>
    /// Gets the current ambient <see cref="IScopeContext"/> from the stack if available; otherwise, returns <c>null</c>.
    /// </summary>
    public IScopeContext? AmbientContext
    {
        get
        {
            lock (_lock)
            {
                if (_stack.Value?.TryPeek(out IScopeContext? ambientContext) ?? false)
                {
                    return ambientContext;
                }

                return null;
            }

        }
    }

    /// <summary>
    /// Removes and returns the current ambient scope context from the stack.
    /// </summary>
    /// <returns>The ambient scope context that was removed from the stack.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is no ambient scope context on the stack.</exception>
    public IScopeContext Pop()
    {
        lock (_lock)
        {
            if (_stack.Value?.TryPop(out IScopeContext? ambientContext) ?? false)
            {
                return ambientContext;
            }

            throw new InvalidOperationException("No AmbientContext was found.");
        }
    }

    /// <summary>
    /// Pushes the specified <see cref="IScopeContext"/> instance onto the ambient scope context stack.
    /// </summary>
    /// <param name="scope">The <see cref="IScopeContext"/> to push onto the stack.</param>
    public void Push(IScopeContext scope)
    {
        lock (_lock)
        {
            _stack.Value ??= new ConcurrentStack<IScopeContext>();

            _stack.Value.Push(scope);
        }
    }
}
