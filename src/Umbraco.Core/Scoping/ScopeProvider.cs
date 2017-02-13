using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;
#if DEBUG_SCOPES
using System.Linq;
#endif

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
                    var scope = GetCallContextObject<IScopeInternal>(ScopeItemKey);
                    var context = GetCallContextObject<ScopeContext>(ContextItemKey);
                    SetCallContextObject(ScopeItemKey, null);
                    SetCallContextObject(ContextItemKey, null);
                    return Tuple.Create(scope, context);
                },
                o =>
                {
                    // cannot re-attached over leaked scope/context
                    // except of course over NoScope (which leaks)
                    var ambientScope = AmbientScopeInternal;
                    if (ambientScope != null)
                    {
                        var ambientNoScope = ambientScope as NoScope;
                        if (ambientNoScope == null)
                            throw new Exception("Found leaked scope when restoring call context.");

                        // this should rollback any pending transaction
                        ambientNoScope.Dispose();
                    }
                    if (AmbientContextInternal != null) throw new Exception("Found leaked context when restoring call context.");

                    var t = (Tuple<IScopeInternal, ScopeContext>) o;
                    SetCallContextObject(ScopeItemKey, t.Item1);
                    SetCallContextObject(ContextItemKey, t.Item2);
                });
        }

        public IDatabaseFactory2 DatabaseFactory { get; private set; }

        #region Context

        // objects that go into the logical call context better be serializable else they'll eventually
        // cause issues whenever some cross-AppDomain code executes - could be due to ReSharper running
        // tests, any other things (see https://msdn.microsoft.com/en-us/library/dn458353(v=vs.110).aspx),
        // but we don't want to make all of our objects serializable since they are *not* meant to be
        // used in cross-AppDomain scenario anyways.
        // in addition, whatever goes into the logical call context is serialized back and forth any
        // time cross-AppDomain code executes, so if we put an "object" there, we'll can *another*
        // "object" instance - and so we cannot use a random object as a key.
        // so what we do is: we register a guid in the call context, and we keep a table mapping those
        // guids to the actual objects. the guid serializes back and forth without causing any issue,
        // and we can retrieve the actual objects from the table.
        // only issue: how are we supposed to clear the table? we can't, really. objects should take
        // care of de-registering themselves from context.
        // everything we use does, except the NoScope scope, which just stays there
        //
        // during tests, NoScope can to into call context... nothing much we can do about it

        private static readonly object StaticCallContextObjectsLock = new object();
        private static readonly Dictionary<Guid, object> StaticCallContextObjects
             = new Dictionary<Guid, object>();

#if DEBUG_SCOPES
        public Dictionary<Guid, object> CallContextObjects
        {
            get
            {
                lock (StaticCallContextObjectsLock)
                {
                    // capture in a dictionary
                    return StaticCallContextObjects.ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
#endif

        private static T GetCallContextObject<T>(string key)
            where T : class
        {
            var objectKey = CallContext.LogicalGetData(key).AsGuid();
            lock (StaticCallContextObjectsLock)
            {
                object callContextObject;
                return StaticCallContextObjects.TryGetValue(objectKey, out callContextObject) ? (T)callContextObject : null;
            }
        }

        private static void SetCallContextObject(string key, object value)
        {
#if DEBUG_SCOPES
            // manage the 'context' that contains the scope (null, "http" or "call")
            // first, null-register the existing value
            var ambientKey = CallContext.LogicalGetData(ScopeItemKey).AsGuid();
            object o = null;
            lock (StaticCallContextObjectsLock)
            {
                if (ambientKey != default (Guid))
                    StaticCallContextObjects.TryGetValue(ambientKey, out o);
            }
            var ambientScope = o as IScope;
            if (ambientScope != null) RegisterContext(ambientScope, null);
            // then register the new value
            var scope = value as IScope;
            if (scope != null) RegisterContext(scope, "call");
#endif
            if (value == null)
            {
                var objectKey = CallContext.LogicalGetData(key).AsGuid();
                CallContext.FreeNamedDataSlot(key);
                if (objectKey == default (Guid)) return;
                lock (StaticCallContextObjectsLock)
                {
                    StaticCallContextObjects.Remove(objectKey);
                }
            }
            else
            {
                // note - we are *not* detecting an already-existing value
                // because our code in this class *always* sets to null before
                // setting to a real value
                var objectKey = Guid.NewGuid();
                lock (StaticCallContextObjectsLock)
                {
                    StaticCallContextObjects.Add(objectKey, value);
                }
                CallContext.LogicalSetData(key, objectKey);
            }
        }

        internal static Func<IDictionary> HttpContextItemsGetter { get; set; }

        private static IDictionary HttpContextItems
        {
            get
            {
                return HttpContextItemsGetter == null
                    ? (HttpContext.Current == null ? null : HttpContext.Current.Items)
                    : HttpContextItemsGetter();
            }
        }

        public static T GetHttpContextObject<T>(string key, bool required = true)
            where T : class
        {
            var httpContextItems = HttpContextItems;
            if (httpContextItems != null)
                return (T)httpContextItems[key];
            if (required)
                throw new Exception("HttpContext.Current is null.");
            return null;
        }

        private static bool SetHttpContextObject(string key, object value, bool required = true)
        {
            var httpContextItems = HttpContextItems;
            if (httpContextItems == null)
            {
                if (required)
                    throw new Exception("HttpContext.Current is null.");
                return false;
            }
#if DEBUG_SCOPES
            // manage the 'context' that contains the scope (null, "http" or "call")
            // first, null-register the existing value
            var ambientScope = (IScope)httpContextItems[ScopeItemKey];
            if (ambientScope != null) RegisterContext(ambientScope, null);
            // then register the new value
            var scope = value as IScope;
            if (scope != null) RegisterContext(scope, "http");
#endif
            if (value == null)
                httpContextItems.Remove(key);
            else
                httpContextItems[key] = value;
            return true;
        }

        #endregion

        #region Ambient Context

        internal const string ContextItemKey = "Umbraco.Core.Scoping.ScopeContext";

        internal static ScopeContext AmbientContextInternal
        {
            get
            {
                // try http context, fallback onto call context
                var value = GetHttpContextObject<ScopeContext>(ContextItemKey, false);
                return value ?? GetCallContextObject<ScopeContext>(ContextItemKey);
            }
            set
            {
                // clear both
                SetHttpContextObject(ContextItemKey, null, false);
                SetCallContextObject(ContextItemKey, null);
                if (value == null) return;

                // set http/call context
                if (SetHttpContextObject(ContextItemKey, value, false) == false)
                    SetCallContextObject(ContextItemKey, value);
            }
        }

        /// <inheritdoc />
        public ScopeContext AmbientContext
        {
            get { return AmbientContextInternal; }
        }

        #endregion

        #region Ambient Scope

        internal const string ScopeItemKey = "Umbraco.Core.Scoping.Scope";
        internal const string ScopeRefItemKey = "Umbraco.Core.Scoping.ScopeReference";

        // only 1 instance which can be disposed and disposed again
        private static readonly ScopeReference StaticScopeReference = new ScopeReference(new ScopeProvider(null));

        internal static IScopeInternal AmbientScopeInternal
        {
            get
            {
                // try http context, fallback onto call context
                var value = GetHttpContextObject<IScopeInternal>(ScopeItemKey, false);
                return value ?? GetCallContextObject<IScopeInternal>(ScopeItemKey);
            }
            set
            {
                // clear both
                SetHttpContextObject(ScopeItemKey, null, false);
                SetHttpContextObject(ScopeRefItemKey, null, false);
                SetCallContextObject(ScopeItemKey, null);
                if (value == null) return;

                // set http/call context
                if (value.CallContext == false && SetHttpContextObject(ScopeItemKey, value, false))
                    SetHttpContextObject(ScopeRefItemKey, StaticScopeReference);
                else
                    SetCallContextObject(ScopeItemKey, value);
            }
        }

        /// <inheritdoc />
        public IScopeInternal AmbientScope
        {
            get { return AmbientScopeInternal; }
            internal set { AmbientScopeInternal = value; }
        }

        /// <inheritdoc />
        public IScopeInternal GetAmbientOrNoScope()
        {
            return AmbientScope ?? (AmbientScope = new NoScope(this));
        }

        #endregion

        public void SetAmbient(IScopeInternal scope, ScopeContext context = null)
        {
            // clear all
            SetHttpContextObject(ScopeItemKey, null, false);
            SetHttpContextObject(ScopeRefItemKey, null, false);
            SetCallContextObject(ScopeItemKey, null);
            SetHttpContextObject(ContextItemKey, null, false);
            SetCallContextObject(ContextItemKey, null);
            if (scope == null)
            {
                if (context != null)
                    throw new ArgumentException("Must be null if scope is null.", "context");
                return;
            }

            if (context == null)
                throw new ArgumentNullException("context");

            if (scope.CallContext == false && SetHttpContextObject(ScopeItemKey, scope, false))
            {
                SetHttpContextObject(ScopeRefItemKey, StaticScopeReference);
                SetHttpContextObject(ContextItemKey, context);
            }
            else
            {
                SetCallContextObject(ScopeItemKey, scope);
                SetCallContextObject(ContextItemKey, context);
            }
        }

        /// <inheritdoc />
        public IScope CreateDetachedScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null)
        {
            return new Scope(this, true, null, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems);
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
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null,
            bool callContext = false)
        {
            var ambient = AmbientScope;
            if (ambient == null)
            {
                var ambientContext = AmbientContext;
                var newContext = ambientContext == null ? new ScopeContext() : null;
                var scope = new Scope(this, false, newContext, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext);
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
                var scope = new Scope(this, noScope, newContext, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext);
                // assign only if scope creation did not throw!
                SetAmbient(scope, newContext ?? ambientContext);
                return scope;
            }

            var ambientScope = ambient as Scope;
            if (ambientScope == null) throw new Exception("Ambient scope is not a Scope instance.");

            var nested = new Scope(this, ambientScope, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext);
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

        public ScopeInfo GetScopeInfo(IScope scope)
        {
            lock (StaticScopeInfosLock)
            {
                return StaticScopeInfos.FirstOrDefault(x => x.Scope == scope);
            }
        }

        //private static void Log(string message, UmbracoDatabase database)
        //{
        //    LogHelper.Debug<ScopeProvider>(message + " (" + (database == null ? "" : database.InstanceSid) + ").");
        //}

        // register a scope and capture its ctor stacktrace
        public void RegisterScope(IScope scope)
        {
            lock (StaticScopeInfosLock)
            {
                if (StaticScopeInfos.Any(x => x.Scope == scope)) throw new Exception("oops: already registered.");
                StaticScopeInfos.Add(new ScopeInfo(scope, Environment.StackTrace));
            }
        }

        // register that a scope is in a 'context'
        // 'context' that contains the scope (null, "http" or "call")
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
