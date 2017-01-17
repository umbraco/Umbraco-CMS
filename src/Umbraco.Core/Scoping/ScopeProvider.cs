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
                throw new ArgumentException("Not a Scope instance.");

            if (otherScope.Detachable == false)
                throw new ArgumentException("Not a detachable scope.");

            otherScope.OrigScope = AmbientScope;
            AmbientScope = otherScope;
        }

        /// <inheritdoc />
        public IScope DetachScope()
        {
            var ambient = AmbientScope;
            if (ambient == null)
                throw new InvalidOperationException("There is no ambient scope.");

            var noScope = ambient as NoScope;
            if (noScope != null)
                throw new InvalidOperationException("Cannot detach NoScope.");

            var scope = ambient as Scope;
            if (scope == null)
                throw new Exception("Ambient scope is not a Scope instance.");

            if (scope.Detachable == false)
                throw new InvalidOperationException("Ambient scope is not detachable.");

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
                    throw new Exception("NoScope is in a transaction.");
                return AmbientScope = new Scope(this, noScope);
            }

            var scope = ambient as Scope;
            if (scope == null) throw new Exception("Ambient scope is not a Scope instance.");

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

#if DEBUG_SCOPES
        // this code needs TLC
        // the idea is to keep in a list all the scopes that have been created
        // and remove them when they are disposed
        // so we can track leaks

        // helps identifying when non-httpContext scopes are created by logging the stack trace
        private void LogCallContextStack()
        {
            var trace = Environment.StackTrace;
            if (trace.IndexOf("ScheduledPublishing") > 0)
                LogHelper.Debug<ScopeProvider>("CallContext: Scheduled Publishing");
            else if (trace.IndexOf("TouchServerTask") > 0)
                LogHelper.Debug<ScopeProvider>("CallContext: Server Registration");
            else if (trace.IndexOf("LogScrubber") > 0)
                LogHelper.Debug<ScopeProvider>("CallContext: Log Scrubber");
            else
                LogHelper.Debug<ScopeProvider>("CallContext: " + Environment.StackTrace);
        }

        private readonly List<IScope> _scopes = new List<IScope>();

        // helps identifying scope leaks by keeping track of all instances
        public List<IScope> Scopes { get { return _scopes; } }

        private static void Log(string message, UmbracoDatabase database)
        {
            LogHelper.Debug<ScopeProvider>(message + " (" + (database == null ? "" : database.InstanceSid) + ").");
        }
#endif

    }
}
