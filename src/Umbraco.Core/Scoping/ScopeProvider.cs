using System;
using System.Data;
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
                    var scope = AmbientContextScope;
                    var context = AmbientContextContext;
                    AmbientContextScope = null;
                    AmbientContextContext = null;
                    return Tuple.Create(scope, context);
                },
                o =>
                {
                    // cannot re-attached over leaked scope/context
                    // except of course over NoScope (which leaks)
                    var ambientScope = AmbientContextScope;
                    if (ambientScope != null)
                    {
                        var ambientNoScope = ambientScope as NoScope;
                        if (ambientNoScope == null)
                            throw new Exception("Found leaked scope when restoring call context.");

                        // this should rollback any pending transaction
                        ambientNoScope.Dispose();
                    }
                    if (AmbientContextContext != null) throw new Exception("Found leaked context when restoring call context.");

                    var t = (Tuple<IScopeInternal, ScopeContext>)o;
                    AmbientContextScope = t.Item1;
                    AmbientContextContext = t.Item2;
                });
        }

        public IDatabaseFactory2 DatabaseFactory { get; private set; }

        #region Ambient Context

        internal const string ContextItemKey = "Umbraco.Core.Scoping.ScopeContext";

        private static ScopeContext CallContextContext
        {
            get { return (ScopeContext)CallContext.LogicalGetData(ContextItemKey); }
            set
            {
                if (value == null) CallContext.FreeNamedDataSlot(ContextItemKey);
                else CallContext.LogicalSetData(ContextItemKey, value);
            }
        }

        private static ScopeContext HttpContextContext
        {
            get { return (ScopeContext)HttpContext.Current.Items[ContextItemKey]; }
            set
            {
                if (value == null)
                    HttpContext.Current.Items.Remove(ContextItemKey);
                else
                    HttpContext.Current.Items[ContextItemKey] = value;
            }
        }

        private static ScopeContext AmbientContextContext
        {
            get
            {
                // try http context, fallback onto call context
                var value = HttpContext.Current == null ? null : HttpContextContext;
                return value ?? CallContextContext;
            }
            set
            {
                // clear both
                if (HttpContext.Current != null)
                    HttpContextContext = value;
                CallContextContext = value;
            }
        }

        /// <inheritdoc />
        public ScopeContext AmbientContext
        {
            get { return AmbientContextContext; }
        }

        #endregion

        #region Ambient Scope

        internal const string ScopeItemKey = "Umbraco.Core.Scoping.Scope";
        internal const string ScopeRefItemKey = "Umbraco.Core.Scoping.ScopeReference";

        // only 1 instance which can be disposed and disposed again
        private static readonly ScopeReference StaticScopeReference = new ScopeReference(new ScopeProvider(null));

        private static IScopeInternal CallContextScope
        {
            get { return (IScopeInternal) CallContext.LogicalGetData(ScopeItemKey); }
            set
            {
#if DEBUG_SCOPES
                // manage the 'context' that contains the scope (null, "http" or "lcc")
                var ambientScope = (IScope) CallContext.LogicalGetData(ScopeItemKey);
                if (ambientScope != null) RegisterContext(ambientScope, null);
                if (value != null) RegisterContext(value, "lcc");
#endif
                if (value == null) CallContext.FreeNamedDataSlot(ScopeItemKey);
                else CallContext.LogicalSetData(ScopeItemKey, value);
            }
        }

        private static IScopeInternal HttpContextScope
        {
            get { return (IScopeInternal) HttpContext.Current.Items[ScopeItemKey]; }
            set
            {
#if DEBUG_SCOPES
                // manage the 'context' that contains the scope (null, "http" or "lcc")
                var ambientScope = (IScope) HttpContext.Current.Items[ScopeItemKey];
                if (ambientScope != null) RegisterContext(ambientScope, null);
                if (value != null) RegisterContext(value, "http");
#endif
                if (value == null)
                {
                    HttpContext.Current.Items.Remove(ScopeItemKey);
                    HttpContext.Current.Items.Remove(ScopeRefItemKey);
                }
                else
                {
                    HttpContext.Current.Items[ScopeItemKey] = value;
                    if (HttpContext.Current.Items[ScopeRefItemKey] == null)
                        HttpContext.Current.Items[ScopeRefItemKey] = StaticScopeReference;
                }
            }
        }

        private static IScopeInternal AmbientContextScope
        {
            get
            {
                // try http context, fallback onto call context
                var value = HttpContext.Current == null ? null : HttpContextScope;
                return value ?? CallContextScope;
            }
            set
            {
                // clear both
                if (HttpContext.Current != null)
                    HttpContextScope = value;
                CallContextScope = value;
            }
        }

        /// <inheritdoc />
        public IScopeInternal AmbientScope
        {
            get { return AmbientContextScope; }
        }

        public void SetAmbientScope(IScopeInternal value)
        {
            if (value != null && value.CallContext)
            {
                if (HttpContext.Current != null)
                    HttpContextScope = null; // clear http context
                CallContextScope = value; // set call context
            }
            else
            {
                CallContextScope = null; // clear call context
                AmbientContextScope = value; // set appropriate context (maybe null)
            }
        }

        /// <inheritdoc />
        public IScopeInternal GetAmbientOrNoScope()
        {
            return AmbientScope ?? (AmbientContextScope = new NoScope(this));
        }

        #endregion

        public void SetAmbient(IScopeInternal scope, ScopeContext context = null)
        {
            if (scope != null && scope.CallContext)
            {
                // clear http context
                if (HttpContext.Current != null)
                {
                    HttpContextScope = null;
                    HttpContextContext = null;
                }

                // set call context
                CallContextScope = scope;
                CallContextContext = context;
            }
            else
            {
                // clear call context
                CallContextScope = null;
                CallContextContext = null;

                // set appropriate context (maybe null)
                AmbientContextScope = scope;
                AmbientContextContext = context;
            }
        }

        /// <inheritdoc />
        public IScope CreateDetachedScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            EventsDispatchMode dispatchMode = EventsDispatchMode.Unspecified,
            bool? scopeFileSystems = null)
        {
            return new Scope(this, true, null, isolationLevel, repositoryCacheMode, dispatchMode, scopeFileSystems);
        }

        /// <inheritdoc />
        public void AttachScope(IScope other, bool callContext = false)
        {
            var otherScope = other as Scope;
            if (otherScope == null)
                throw new ArgumentException("Not a Scope instance.");

            if (otherScope.Detachable == false)
                throw new ArgumentException("Not a detachable scope.");

            otherScope.OrigScope = AmbientScope;
            otherScope.OrigContext = AmbientContext;

            otherScope.CallContext = callContext;
            SetAmbient(otherScope, otherScope.Context);
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

            SetAmbient(scope.OrigScope, scope.OrigContext);
            scope.OrigScope = null;
            scope.OrigContext = null;
            return scope;
        }

        /// <inheritdoc />
        public IScope CreateScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            EventsDispatchMode dispatchMode = EventsDispatchMode.Unspecified,
            bool? scopeFileSystems = null,
            bool callContext = false)
        {
            var ambient = AmbientScope;
            if (ambient == null)
            {
                var ambientContext = AmbientContext;
                var newContext = ambientContext == null ? new ScopeContext() : null;
                var scope = new Scope(this, false, newContext, isolationLevel, repositoryCacheMode, dispatchMode, scopeFileSystems, callContext);
                // assign only if scope creation did not throw!
                SetAmbient(scope, newContext ?? ambientContext);
                return scope;
            }

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
                var ambientContext = AmbientContext;
                var newContext = ambientContext == null ? new ScopeContext() : null;
                var scope = new Scope(this, noScope, newContext, isolationLevel, repositoryCacheMode, dispatchMode, scopeFileSystems, callContext);
                // assign only if scope creation did not throw!
                SetAmbient(scope, newContext ?? ambientContext);
                return scope;
            }

            var ambientScope = ambient as Scope;
            if (ambientScope == null) throw new Exception("Ambient scope is not a Scope instance.");

            var nested = new Scope(this, ambientScope, isolationLevel, repositoryCacheMode, dispatchMode, scopeFileSystems, callContext);
            SetAmbient(nested, AmbientContext);
            return nested;
        }

        /// <inheritdoc />
        public void Reset()
        {
            var scope = AmbientScope as Scope;
            if (scope != null)
                scope.Reset();

            StaticScopeReference.Dispose();
        }

        /// <inheritdoc />
        public ScopeContext Context
        {
            get { return AmbientContext; }
        }

#if DEBUG_SCOPES
        // this code needs TLC
        //
        // the idea here is to keep in a list all the scopes that have been created, and to remove them
        // when they are disposed, so we can track leaks, ie scopes that would not be properly taken
        // care of by our code
        //
        // note: the code could probably be optimized... but this is NOT supposed to go into any real
        // live build, either production or debug - it's just a debugging tool for the time being

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

        // all scope instances that are currently beeing tracked
        private static readonly object StaticScopeInfosLock = new object();
        private static readonly List<ScopeInfo> StaticScopeInfos = new List<ScopeInfo>();

        public IEnumerable<ScopeInfo> ScopeInfos
        {
            get
            {
                lock (StaticScopeInfosLock)
                {
                    return StaticScopeInfos.ToArray(); // capture in an array
                }
            }
        }

        //private static void Log(string message, UmbracoDatabase database)
        //{
        //    LogHelper.Debug<ScopeProvider>(message + " (" + (database == null ? "" : database.InstanceSid) + ").");
        //}

        public void RegisterScope(IScope scope)
        {
            lock (StaticScopeInfosLock)
            {
                if (StaticScopeInfos.Any(x => x.Scope == scope)) throw new Exception("oops: already registered.");
                StaticScopeInfos.Add(new ScopeInfo(scope, Environment.StackTrace));
            }
        }

        // 'context' that contains the scope (null, "http" or "lcc")
        public static void RegisterContext(IScope scope, string context)
        {
            lock (StaticScopeInfosLock)
            {
                var info = StaticScopeInfos.FirstOrDefault(x => x.Scope == scope);
                if (info == null)
                {
                    if (context == null) return;
                    throw new Exception("oops: unregistered scope.");
                }
                if (context == null) info.NullStack = Environment.StackTrace;
                info.Context = context;
            }
        }

        public void Disposed(IScope scope)
        {
            lock (StaticScopeInfosLock)
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

        public IScope Scope { get; private set; } // the scope itself

        // the scope's parent identifier
        public Guid Parent { get { return (Scope is NoScope || ((Scope) Scope).ParentScope == null) ? Guid.Empty : ((Scope) Scope).ParentScope.InstanceId; } }

        public DateTime Created { get; private set; } // the date time the scope was created
        public bool Disposed { get; set; } // whether the scope has been disposed already
        public string Context { get; set; } // the current 'context' that contains the scope (null, "http" or "lcc")

        public string CtorStack { get; private set; } // the stacktrace of the scope ctor
        public string DisposedStack { get; set; } // the stacktrace when disposed
        public string NullStack { get; set; } // the stacktrace when the 'context' that contains the scope went null
    }
#endif
}
