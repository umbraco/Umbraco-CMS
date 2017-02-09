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
                    var context = StaticAmbientContext;
                    StaticAmbientScope = null;
                    StaticAmbientContext = null;
                    return Tuple.Create(scope, context);
                },
                o =>
                {
                    // cannot re-attached over leaked scope/context
                    // except of course over NoScope (which leaks)
                    var ambientScope = StaticAmbientScope;
                    if (ambientScope != null)
                    {
                        var ambientNoScope = ambientScope as NoScope;
                        if (ambientNoScope == null)
                            throw new Exception("Found leaked scope when restoring call context.");

                        // this should rollback any pending transaction
                        ambientNoScope.Dispose();
                    }
                    if (StaticAmbientContext != null) throw new Exception("Found leaked context when restoring call context.");

                    var t = (Tuple<IScopeInternal, ScopeContext>)o;
                    StaticAmbientScope = t.Item1;
                    StaticAmbientContext = t.Item2;
                });
        }

        public IDatabaseFactory2 DatabaseFactory { get; private set; }

        #region Ambient Context

        internal const string ContextItemKey = "Umbraco.Core.Scoping.ScopeContext";

        private static ScopeContext CallContextContextValue
        {
            get { return (ScopeContext)CallContext.LogicalGetData(ContextItemKey); }
            set
            {
                if (value == null) CallContext.FreeNamedDataSlot(ContextItemKey);
                else CallContext.LogicalSetData(ContextItemKey, value);
            }
        }

        private static ScopeContext HttpContextContextValue
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

        private static ScopeContext StaticAmbientContext
        {
            get { return HttpContext.Current == null ? CallContextContextValue : HttpContextContextValue; }
            set
            {
                if (HttpContext.Current == null)
                    CallContextContextValue = value;
                else
                    HttpContextContextValue = value;
            }
        }

        /// <inheritdoc />
        public ScopeContext AmbientContext
        {
            get { return StaticAmbientContext; }
            set { StaticAmbientContext = value; }
        }

        #endregion

        #region Ambient Scope

        internal const string ScopeItemKey = "Umbraco.Core.Scoping.Scope";
        internal const string ScopeRefItemKey = "Umbraco.Core.Scoping.ScopeReference";

        // only 1 instance which can be disposed and disposed again
        private static readonly ScopeReference StaticScopeReference = new ScopeReference(new ScopeProvider(null));

        private static IScopeInternal CallContextValue
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

        private static IScopeInternal HttpContextValue
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

        private static IScopeInternal StaticAmbientScope
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
        public IScopeInternal AmbientScope
        {
            get { return StaticAmbientScope; }
            set { StaticAmbientScope = value; }
        }

        /// <inheritdoc />
        public IScopeInternal GetAmbientOrNoScope()
        {
                return AmbientScope ?? (AmbientScope = new NoScope(this));
        }

        #endregion

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
        public void AttachScope(IScope other)
        {
            var otherScope = other as Scope;
            if (otherScope == null)
                throw new ArgumentException("Not a Scope instance.");

            if (otherScope.Detachable == false)
                throw new ArgumentException("Not a detachable scope.");

            otherScope.OrigScope = AmbientScope;
            otherScope.OrigContext = AmbientContext;
            AmbientScope = otherScope;
            AmbientContext = otherScope.Context;
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
            AmbientContext = scope.OrigContext;
            scope.OrigScope = null;
            scope.OrigContext = null;
            return scope;
        }

        /// <inheritdoc />
        public IScope CreateScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            EventsDispatchMode dispatchMode = EventsDispatchMode.Unspecified,
            bool? scopeFileSystems = null)
        {
            var ambient = AmbientScope;
            if (ambient == null)
            {
                var context = AmbientContext == null ? new ScopeContext() : null;
                var scope = new Scope(this, false, context, isolationLevel, repositoryCacheMode, dispatchMode, scopeFileSystems);
                if (AmbientContext == null) AmbientContext = context; // assign only if scope creation did not throw!
                return AmbientScope = scope;
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
                var context = AmbientContext == null ? new ScopeContext() : null;
                var scope = new Scope(this, noScope, context, isolationLevel, repositoryCacheMode, dispatchMode, scopeFileSystems);
                if (AmbientContext == null) AmbientContext = context; // assign only if scope creation did not throw!
                return AmbientScope = scope;
            }

            var ambientScope = ambient as Scope;
            if (ambientScope == null) throw new Exception("Ambient scope is not a Scope instance.");

            return AmbientScope = new Scope(this, ambientScope, isolationLevel, repositoryCacheMode, dispatchMode, scopeFileSystems);
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
