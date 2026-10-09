using System.Collections.Concurrent;

namespace Umbraco.Cms.Infrastructure.Scoping
{
    internal sealed class AmbientScopeStack : IAmbientScopeStack
    {
        private static Lock _lock = new();
        private static AsyncLocal<ConcurrentStack<IScope>> _stack = new ();

        /// <summary>
        /// Gets the current ambient scope if one exists; otherwise, returns null.
        /// </summary>
        public IScope? AmbientScope
        {
            get
            {
                lock (_lock)
                {
                    if (_stack.Value?.TryPeek(out IScope? ambientScope) ?? false)
                    {
                        return ambientScope;
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Removes and returns the ambient scope from the stack.
        /// </summary>
        /// <returns>The ambient scope that was removed from the stack.</returns>
        /// <exception cref="InvalidOperationException">Thrown if there is no ambient scope on the stack to remove.</exception>
        public IScope Pop()
        {
            lock (_lock)
            {


                if (_stack.Value?.TryPop(out IScope? ambientScope) ?? false)
                {
                    return ambientScope;
                }

                throw new InvalidOperationException("No AmbientScope was found.");
            }
        }

        /// <summary>
        /// Pushes the specified scope onto the ambient scope stack.
        /// </summary>
        /// <param name="scope">The scope to push onto the stack.</param>
        public void Push(IScope scope)
        {
            lock (_lock)
            {
                ConcurrentStack<IScope>? stack = _stack.Value;

                // An empty stack was inherited from an ancestor execution context, because popping the last entry
                // leaves the instance in place. AsyncLocal copy-on-write protects the reference and not the instance
                // it points at, so pushing onto it would share this flow's scopes with every sibling flow that
                // inherited it. A non-empty stack is genuine nesting within this flow and is kept.
                if (stack is null || stack.IsEmpty)
                {
                    stack = new ConcurrentStack<IScope>();
                    _stack.Value = stack;
                }

                stack.Push(scope);
            }
        }
    }
}
