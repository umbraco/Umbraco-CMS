using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Umbraco.Core.Events;
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
#if DEBUG_SCOPES
                var ambient = (IScope)CallContext.LogicalGetData(ItemKey);
                if (ambient != null) RegisterContext(ambient, null);
                if (value != null)
                    RegisterContext(value, "lcc");
#endif
                if (value == null) CallContext.FreeNamedDataSlot(ItemKey);
                else CallContext.LogicalSetData(ItemKey, value);
            }
        }

        private static IScope HttpContextValue
        {
            get { return (IScope) HttpContext.Current.Items[ItemKey]; }
            set
            {
#if DEBUG_SCOPES
                var ambient = (IScope) HttpContext.Current.Items[ItemKey];
                if (ambient != null) RegisterContext(ambient, null);
                if (value != null)
                    RegisterContext(value, "http");
#endif
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
        public IScope CreateDetachedScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified, 
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            EventsDispatchMode dispatchMode = EventsDispatchMode.Unspecified)
        {
            return new Scope(this, isolationLevel, repositoryCacheMode, dispatchMode, true);
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
        public IScope CreateScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified, 
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            EventsDispatchMode dispatchMode = EventsDispatchMode.Unspecified)
        {
            var ambient = AmbientScope;
            if (ambient == null)
                return AmbientScope = new Scope(this, isolationLevel, repositoryCacheMode, dispatchMode);

            // replace noScope with a real one
            var noScope = ambient as NoScope;
            if (noScope != null)
            {
#if DEBUG_SCOPES
                Disposed(noScope);
#endif
                // peta poco nulls the shared connection after each command unless there's a trx
                var database = noScope.DatabaseOrNull;
                if (database != null && database.InTransaction)
                    throw new Exception("NoScope is in a transaction.");
                return AmbientScope = new Scope(this, noScope, isolationLevel, repositoryCacheMode, dispatchMode);
            }

            var scope = ambient as Scope;
            if (scope == null) throw new Exception("Ambient scope is not a Scope instance.");

            return AmbientScope = new Scope(this, scope, isolationLevel, repositoryCacheMode, dispatchMode);
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
        //private void LogCallContextStack()
        //{
        //    var trace = Environment.StackTrace;
        //    if (trace.IndexOf("ScheduledPublishing") > 0)
        //        LogHelper.Debug<ScopeProvider>("CallContext: Scheduled Publishing");
        //    else if (trace.IndexOf("TouchServerTask") > 0)
        //        LogHelper.Debug<ScopeProvider>("CallContext: Server Registration");
        //    else if (trace.IndexOf("LogScrubber") > 0)
        //        LogHelper.Debug<ScopeProvider>("CallContext: Log Scrubber");
        //    else
        //        LogHelper.Debug<ScopeProvider>("CallContext: " + Environment.StackTrace);
        //}

        // helps identifying scope leaks by keeping track of all instances
        public static readonly List<ScopeInfo> StaticScopeInfos = new List<ScopeInfo>();

        public IEnumerable<ScopeInfo> ScopeInfos { get { return StaticScopeInfos; } }

        //private static void Log(string message, UmbracoDatabase database)
        //{
        //    LogHelper.Debug<ScopeProvider>(message + " (" + (database == null ? "" : database.InstanceSid) + ").");
        //}

        public void Register(IScope scope)
        {
            if (StaticScopeInfos.Any(x => x.Scope == scope)) throw new Exception();
            StaticScopeInfos.Add(new ScopeInfo(scope, Environment.StackTrace));
        }

        public static void RegisterContext(IScope scope, string context)
        {
            var info = StaticScopeInfos.FirstOrDefault(x => x.Scope == scope);
            if (info == null)
            {
                if (context == null) return;
                throw new Exception();
            }
            if (context == null) info.NullStack = Environment.StackTrace;
            info.Context = context;
        }

        public void Disposed(IScope scope)
        {
            var info = StaticScopeInfos.FirstOrDefault(x => x.Scope == scope);
            if (info != null)
            {
                // enable this by default
                StaticScopeInfos.Remove(info);

                // instead, enable this to keep *all* scopes
                // beware, there can be a lot of scopes!
                //info.Disposed = true;
                //info.DisposedStack = Environment.StackTrace;
            }
        }
#endif
    }

#if DEBUG_SCOPES
    public class ScopeInfo
    {
        public ScopeInfo(IScope scope, string ctorStack)
        {
            Scope = scope;
            Created = DateTime.Now;
            CtorStack = ctorStack;
        }

        public IScope Scope { get; private set; }
        public Guid Parent { get { return (Scope is NoScope || ((Scope) Scope).ParentScope == null) ? Guid.Empty : ((Scope) Scope).ParentScope.InstanceId; } }
        public DateTime Created { get; private set; }
        public bool Disposed { get; set; }
        public string Context { get; set; }
        public string CtorStack { get; private set; } // the stacktrace of the scope ctor
        public string DisposedStack { get; set; } // the stacktrace when disposed
        public string NullStack { get; set; } // the stacktrace when context went null
    }
#endif
}
