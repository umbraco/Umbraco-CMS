using System.Collections.Concurrent;

namespace Umbraco.Cms.Infrastructure.Scoping
{
    internal class AmbientScopeStack : IAmbientScopeStack
    {
        private static AsyncLocal<ConcurrentStack<IScope>> _stack = new ();

        public IScope? AmbientScope
        {
            get
            {
                if (_stack.Value?.TryPeek(out IScope? ambientScope) ?? false)
                {
                    return ambientScope;
                }

                return null;
            }
        }

        public IScope Pop()
        {
            if (_stack.Value?.TryPop(out IScope? ambientScope) ?? false)
            {
                return ambientScope;
            }

            throw new InvalidOperationException("No AmbientScope was found.");
        }

        public void Push(IScope scope)
        {
            _stack.Value ??= new ConcurrentStack<IScope>();

            _stack.Value.Push(scope);
        }
    }
}
