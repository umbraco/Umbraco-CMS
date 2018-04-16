using System;

namespace Umbraco.Core.Scoping
{
    // base class for an object that will be enlisted in scope context, if any. it
    // must be used in a 'using' block, and if not scoped, released when disposed,
    // else when scope context runs enlisted actions
    public abstract class ScopeContextualBase : IDisposable
    {
        private bool _using, _scoped;

        public static T Get<T>(IScopeProvider scopeProvider, string key, Func<bool, T> ctor)
            where T : ScopeContextualBase
        {
            var scopeContext = scopeProvider.Context;
            if (scopeContext == null)
                return ctor(false);

            var w = scopeContext.Enlist("ScopeContextualBase_" + key,
                () => ctor(true),
                (completed, item) => { item.Release(completed); });

            if (w._using) throw new InvalidOperationException("panic: used.");
            w._using = true;
            w._scoped = true;

            return w;
        }

        public void Dispose()
        {
            _using = false;

            if (_scoped == false)
                Release(true);
        }

        public abstract void Release(bool completed);
    }
}
