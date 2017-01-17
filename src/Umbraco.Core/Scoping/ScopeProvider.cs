using System;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Implements <see cref="IScopeProvider"/>.
    /// </summary>
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
                    StaticAmbientScope = (IScope)scope;
                });
        }

        public IDatabaseFactory2 DatabaseFactory { get; private set; }

        private const string ItemKey = "Umbraco.Core.Scoping.IScope";
        private const string ItemRefKey = "Umbraco.Core.Scoping.ScopeReference";

        // only 1 instance which can be disposed and disposed again
        private static readonly ScopeReference StaticScopeReference = new ScopeReference(new ScopeProvider(null));

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
                if (value == null)
                {
                    HttpContext.Current.Items.Remove(ItemKey);
                    HttpContext.Current.Items.Remove(ItemRefKey);
                }
                else
                {
                    HttpContext.Current.Items[ItemKey] = value;
                    if (HttpContext.Current.Items[ItemRefKey] == null)
                        HttpContext.Current.Items[ItemRefKey] = StaticScopeReference;
                }
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

        /// <inheritdoc />
        public void Reset()
        {
            var scope = AmbientScope as Scope;
            if (scope != null)
                scope.Reset();

            StaticScopeReference.Dispose();
        }
    }
}
