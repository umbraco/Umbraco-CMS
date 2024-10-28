using System.Collections.Concurrent;

namespace Umbraco.Cms.Infrastructure.Scoping
{
    internal class AmbientScopeStack : IAmbientScopeStack
    {
        private static Lock _lock = new();
        private static AsyncLocal<ConcurrentStack<IScope>> _stack = new ();

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

        public void Push(IScope scope)
        {
            lock (_lock)
            {
                (_stack.Value ??= new ConcurrentStack<IScope>()).Push(scope);
            }
        }
    }
}
