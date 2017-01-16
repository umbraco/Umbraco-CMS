using System;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    internal class ScopeProvider : IScopeProviderInternal
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

        /// <inheritdoc />
        public IScope AmbientScope
        {
            get { return StaticAmbientScope; }
            set { StaticAmbientScope = value; }
        }

        /// <inheritdoc />
        public IScope AmbientOrNoScope
        {
            get
            {
                return AmbientScope ?? (AmbientScope = new NoScope(this));
            }
        }

        /// <inheritdoc />
        public IScope CreateDetachedScope()
        {
            return new Scope(this, true);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IScope CreateScope()
        {
            var ambient = AmbientScope;
            if (ambient == null)
                return AmbientScope = new Scope(this);

            // replace noScope with a real one
            var noScope = ambient as NoScope;
            if (noScope != null)
            {
                // peta poco nulls the shared connection after each command unless there's a trx
                var database = noScope.DatabaseOrNull;
                if (database != null && database.InTransaction)
                    throw new Exception();
                return AmbientScope = new Scope(this, noScope);
            }

            var scope = ambient as Scope;
            if (scope == null) throw new Exception();

            return AmbientScope = new Scope(this, scope);
        }

        public void Disposing(IScope disposing, bool? completed = null)
        {
            if (disposing != AmbientScope)
                throw new InvalidOperationException();

            var noScope = disposing as NoScope;
            if (noScope != null)
            {
                // fixme - kinda legacy
                var noScopeDatabase = noScope.DatabaseOrNull;
                if (noScopeDatabase != null)
                {
                    if (noScopeDatabase.InTransaction)
                        throw new Exception();
                    noScopeDatabase.Dispose();
                }
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
            // fixme - when completing... the db should be released, no need to dispose the db?

            // note - messages
            // at the moment we are totally not filtering the messages based on completion
            // status, so whether the scope is committed or rolled back makes no difference

            var database = scope.DatabaseOrNull;
            if (database == null) return;

            if (completed.HasValue && completed.Value)
                database.CompleteTransaction();
            else
                database.AbortTransaction();
        }
    }
}
