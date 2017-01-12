using System;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    internal class ScopeProvider : IScopeProvider
    {
        public ScopeProvider(IDatabaseFactory2 databaseFactory)
        {
            DatabaseFactory = databaseFactory;
        }

        static ScopeProvider()
        {
            SafeCallContext.Register(
                () =>
                {
                    var scope = StaticAmbientScope;
                    StaticAmbientScope = null;
                    return scope;
                },
                scope =>
                {
                    var ambient = StaticAmbientScope;
                    if (ambient != null)
                        ambient.Dispose();
                    StaticAmbientScope = (IScope) scope;
                });
        }

        public IDatabaseFactory2 DatabaseFactory { get; private set; }

        private const string ItemKey = "Umbraco.Core.Scoping.IScope";

        private static IScope CallContextValue
        {
            get { return (IScope) CallContext.LogicalGetData(ItemKey); }
            set
            {
                if (value == null) CallContext.FreeNamedDataSlot(ItemKey);
                else CallContext.LogicalSetData(ItemKey, value);
            }
        }

        private static IScope HttpContextValue
        {
            get { return (IScope) HttpContext.Current.Items[ItemKey]; }
            set
            {
                if (value == null) HttpContext.Current.Items.Remove(ItemKey);
                else HttpContext.Current.Items[ItemKey] = value;
            }
        }

        private static IScope StaticAmbientScope
        {
            get { return HttpContext.Current == null ? CallContextValue : HttpContextValue; }
            set
            {
                if (HttpContext.Current == null)
                    CallContextValue = value;
                else
                    HttpContextValue = value;
            }
        }

        public IScope AmbientScope
        {
            get { return StaticAmbientScope; }
            set { StaticAmbientScope = value; }
        }

        // fixme should we do...
        // using (var s = scopeProvider.AttachScope(other))
        // {
        // }
        // can't because disposing => detach or commit? cannot tell!
        // var scope = scopeProvider.CreateScope();
        // scope = scopeProvider.Detach();
        // scope.Detach();
        // scopeProvider.Attach(scope);
        // ... do things ...
        // scopeProvider.Detach();
        // scopeProvider.Attach(scope);
        // scope.Dispose();

        public IScope CreateDetachedScope()
        {
            return new Scope(this, true);
        }

        public void AttachScope(IScope other)
        {
            var otherScope = other as Scope;
            if (otherScope == null)
                throw new ArgumentException();

            if (otherScope.Detachable == false)
                throw new ArgumentException();

            var ambient = AmbientScope;
            if (ambient == null)
            {
                AmbientScope = other;
                return;
            }

            var noScope = ambient as NoScope;
            if (noScope != null)
                throw new InvalidOperationException();

            var scope = ambient as Scope;
            if (ambient != null && scope == null)
                throw new Exception();

            otherScope.OrigScope = scope;
            AmbientScope = otherScope;
        }

        public IScope DetachScope()
        {
            var ambient = AmbientScope;
            if (ambient == null)
                throw new InvalidOperationException();

            var noScope = ambient as NoScope;
            if (noScope != null)
                throw new InvalidOperationException();

            var scope = ambient as Scope;
            if (scope == null)
                throw new Exception();

            if (scope.Detachable == false)
                throw new InvalidOperationException();

            AmbientScope = scope.OrigScope;
            scope.OrigScope = null;
            return scope;
        }

        public IScope CreateScope()
        {
            var ambient = AmbientScope;
            if (ambient == null)
                return AmbientScope = new Scope(this);

            var noScope = ambient as NoScope;
            if (noScope != null)
            {
                // peta poco nulls the shared connection after each command unless there's a trx
                if (noScope.HasDatabase && noScope.Database.Connection != null)
                    throw new Exception();
                return AmbientScope = new Scope(this, noScope);
            }

            var scope = ambient as Scope;
            if (scope == null) throw new Exception();

            return AmbientScope = new Scope(this, scope);
        }

        public IScope CreateNoScope()
        {
            var ambient = AmbientScope;
            if (ambient != null) throw new Exception();
            return AmbientScope = new NoScope(this);
        }

        public void Disposing(IScope disposing, bool? completed = null)
        {
            if (disposing != AmbientScope)
                throw new InvalidOperationException();

            var noScope = disposing as NoScope;
            if (noScope != null)
            {
                // fixme - kinda legacy
                if (noScope.HasDatabase) noScope.Database.Dispose();
                AmbientScope = null;
                return;
            }

            var scope = disposing as Scope;
            if (scope == null)
                throw new Exception();

            var parent = scope.ParentScope;
            AmbientScope = parent;

            if (parent != null)
            {
                parent.CompleteChild(completed);
                return;
            }

            // fixme - a scope is in a transaction only if ... there is a db transaction, or always?
            // what shall we do with events if not in a transaction?

            // note - messages
            // at the moment we are totally not filtering the messages based on completion
            // status, so whether the scope is committed or rolled back makes no difference

            if (completed.HasValue && completed.Value)
            {
                var database = scope.HasDatabase ? scope.Database : null;
                if (database != null)
                {
                    database.CompleteTransaction();
                }
            }
            else
            {
                var database = scope.HasDatabase ? scope.Database : null;
                if (database != null)
                {
                    database.AbortTransaction();
                }
            }
        }
    }
}
