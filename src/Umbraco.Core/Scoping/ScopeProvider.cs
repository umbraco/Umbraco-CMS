using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

#if DEBUG_SCOPES
using System.Linq;
#endif

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Implements <see cref="IScopeProvider"/>.
    /// </summary>
    internal class ScopeProvider : IScopeProvider, IScopeAccessor
    {
        private readonly ILogger _logger;
        private readonly ICoreDebug _coreDebug;
        private readonly FileSystems _fileSystems;

        public ScopeProvider(IUmbracoDatabaseFactory databaseFactory, FileSystems fileSystems, ILogger logger, ICoreDebug coreDebug)
        {
            DatabaseFactory = databaseFactory;
            _fileSystems = fileSystems;
            _logger = logger;
            _coreDebug = coreDebug;

            // take control of the FileSystems
            _fileSystems.IsScoped = () => AmbientScope != null && AmbientScope.ScopedFileSystems;

            _scopeReference = new ScopeReference(this);
        }

        static ScopeProvider()
        {
            SafeCallContext.Register(
                () =>
                {
                    var scope = GetCallContextObject<Scope>(ScopeItemKey);
                    var context = GetCallContextObject<ScopeContext>(ContextItemKey);
                    SetCallContextObject(ScopeItemKey, null);
                    SetCallContextObject(ContextItemKey, null);
                    return Tuple.Create(scope, context);
                },
                o =>
                {
                    // cannot re-attached over leaked scope/context
                    if (GetCallContextObject<Scope>(ScopeItemKey) != null)
                            throw new Exception("Found leaked scope when restoring call context.");
                    if (GetCallContextObject<ScopeContext>(ContextItemKey) != null)
                        throw new Exception("Found leaked context when restoring call context.");

                    var t = (Tuple<Scope, ScopeContext>) o;
                    SetCallContextObject(ScopeItemKey, t.Item1);
                    SetCallContextObject(ContextItemKey, t.Item2);
                });
        }

        public IUmbracoDatabaseFactory DatabaseFactory { get; }

        public ISqlContext SqlContext => DatabaseFactory.SqlContext;

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
            if (objectKey == Guid.Empty) return null;

            lock (StaticCallContextObjectsLock)
            {
                if (StaticCallContextObjects.TryGetValue(objectKey, out object callContextObject))
                {
#if DEBUG_SCOPES
                    Current.Logger.Debug<ScopeProvider>("Got " + typeof(T).Name + " Object " + objectKey.ToString("N").Substring(0, 8));
                    //_logger.Debug<ScopeProvider>("At:\r\n" + Head(Environment.StackTrace, 24));
#endif
                    return (T)callContextObject;
                }

                // hard to inject into a static method :(
                Current.Logger.Warn<ScopeProvider, string, string>("Missed {TypeName} Object {ObjectKey}", typeof(T).Name, objectKey.ToString("N").Substring(0, 8));
#if DEBUG_SCOPES
                //Current.Logger.Debug<ScopeProvider>("At:\r\n" + Head(Environment.StackTrace, 24));
#endif
                return null;
            }
        }

        private static void SetCallContextObject(string key, IInstanceIdentifiable value)
        {
#if DEBUG_SCOPES
            // manage the 'context' that contains the scope (null, "http" or "call")
            // only for scopes of course!
            if (key == ScopeItemKey)
            {
                // first, null-register the existing value
                var ambientKey = CallContext.LogicalGetData(ScopeItemKey).AsGuid();
                object o = null;
                lock (StaticCallContextObjectsLock)
                {
                    if (ambientKey != default(Guid))
                        StaticCallContextObjects.TryGetValue(ambientKey, out o);
                }
                var ambientScope = o as IScope;
                if (ambientScope != null) RegisterContext(ambientScope, null);
                // then register the new value
                var scope = value as IScope;
                if (scope != null) RegisterContext(scope, "call");
            }
#endif
            if (value == null)
            {
                var objectKey = CallContext.LogicalGetData(key).AsGuid();
                CallContext.FreeNamedDataSlot(key);
                if (objectKey == default) return;
                lock (StaticCallContextObjectsLock)
                {
#if DEBUG_SCOPES
                    Current.Logger.Debug<ScopeProvider>("Remove Object " + objectKey.ToString("N").Substring(0, 8));
                    //Current.Logger.Debug<ScopeProvider>("At:\r\n" + Head(Environment.StackTrace, 24));
#endif
                    StaticCallContextObjects.Remove(objectKey);
                }
            }
            else
            {
                // note - we are *not* detecting an already-existing value
                // because our code in this class *always* sets to null before
                // setting to a real value
                var objectKey = value.InstanceId;
                lock (StaticCallContextObjectsLock)
                {
#if DEBUG_SCOPES
                    Current.Logger.Debug<ScopeProvider>("AddObject " + objectKey.ToString("N").Substring(0, 8));
                    //Current.Logger.Debug<ScopeProvider>("At:\r\n" + Head(Environment.StackTrace, 24));
#endif
                    StaticCallContextObjects.Add(objectKey, value);
                }
                CallContext.LogicalSetData(key, objectKey);
            }
        }

        // this is for tests exclusively until we have a proper accessor in v8
        internal static Func<IDictionary> HttpContextItemsGetter { get; set; }

        private static IDictionary HttpContextItems => HttpContextItemsGetter == null
            ? HttpContext.Current?.Items
            : HttpContextItemsGetter();

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
            // only for scopes of course!
            if (key == ScopeItemKey)
            {
                // first, null-register the existing value
                var ambientScope = (IScope)httpContextItems[ScopeItemKey];
                if (ambientScope != null) RegisterContext(ambientScope, null);
                // then register the new value
                var scope = value as IScope;
                if (scope != null) RegisterContext(scope, "http");
            }
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

        public ScopeContext AmbientContext
        {
            get
            {
                // try http context, fallback onto call context
                var value = GetHttpContextObject<ScopeContext>(ContextItemKey, false);
                return value ?? GetCallContextObject<ScopeContext>(ContextItemKey);
            }

            [Obsolete("This setter is not used and will be removed in future versions")]
            [EditorBrowsable(EditorBrowsableState.Never)]
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

        #endregion

        #region Ambient Scope

        internal const string ScopeItemKey = "Umbraco.Core.Scoping.Scope";
        internal const string ScopeRefItemKey = "Umbraco.Core.Scoping.ScopeReference";

        // only 1 instance which can be disposed and disposed again
        private readonly ScopeReference _scopeReference;

        IScope IScopeAccessor.AmbientScope => AmbientScope;

        // null if there is none
        public Scope AmbientScope
        {
            // try http context, fallback onto call context
            get => GetHttpContextObject<Scope>(ScopeItemKey, false)
                   ?? GetCallContextObject<Scope>(ScopeItemKey);
            set
            {
                // clear both
                SetHttpContextObject(ScopeItemKey, null, false);
                SetHttpContextObject(ScopeRefItemKey, null, false);
                SetCallContextObject(ScopeItemKey, null);
                if (value == null) return;

                // set http/call context
                if (value.CallContext == false && SetHttpContextObject(ScopeItemKey, value, false))
                    SetHttpContextObject(ScopeRefItemKey, _scopeReference);
                else
                    SetCallContextObject(ScopeItemKey, value);
            }
        }

        #endregion

        public void SetAmbient(Scope scope, ScopeContext context = null)
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
                    throw new ArgumentException("Must be null if scope is null.", nameof(context));
                return;
            }

            if (scope.CallContext == false && SetHttpContextObject(ScopeItemKey, scope, false))
            {
                SetHttpContextObject(ScopeRefItemKey, _scopeReference);
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
            return new Scope(this, _logger, _fileSystems, true, null, _coreDebug, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems);
        }

        /// <inheritdoc />
        public void AttachScope(IScope other, bool callContext = false)
        {
            // IScopeProvider.AttachScope works with an IScope
            // but here we can only deal with our own Scope class
            if (!(other is Scope otherScope))
                throw new ArgumentException("Not a Scope instance.");

            if (otherScope.Detachable == false)
                throw new ArgumentException("Not a detachable scope.");

            if (otherScope.Attached)
                throw new InvalidOperationException("Already attached.");

            otherScope.Attached = true;
            otherScope.OrigScope = AmbientScope;
            otherScope.OrigContext = AmbientContext;

            otherScope.CallContext = callContext;
            SetAmbient(otherScope, otherScope.Context);
        }

        /// <inheritdoc />
        public IScope DetachScope()
        {
            var ambientScope = AmbientScope;
            if (ambientScope == null)
                throw new InvalidOperationException("There is no ambient scope.");

            if (ambientScope.Detachable == false)
                throw new InvalidOperationException("Ambient scope is not detachable.");

            SetAmbient(ambientScope.OrigScope, ambientScope.OrigContext);
            ambientScope.OrigScope = null;
            ambientScope.OrigContext = null;
            ambientScope.Attached = false;
            return ambientScope;
        }

        /// <inheritdoc />
        public IScope CreateScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false)
        {
            var ambientScope = AmbientScope;
            if (ambientScope == null)
            {
                var ambientContext = AmbientContext;
                var newContext = ambientContext == null ? new ScopeContext() : null;
                var scope = new Scope(this, _logger, _fileSystems, false, newContext, _coreDebug, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext, autoComplete);
                // assign only if scope creation did not throw!
                SetAmbient(scope, newContext ?? ambientContext);
                return scope;
            }

            var nested = new Scope(this, _logger, _fileSystems, ambientScope, _coreDebug, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext, autoComplete);
            SetAmbient(nested, AmbientContext);
            return nested;
        }

        public void Reset()
        {
            var scope = AmbientScope;
            scope?.Reset();

            _scopeReference.Dispose();
        }

        /// <inheritdoc />
        public IScopeContext Context => AmbientContext;

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

        // all scope instances that are currently being tracked
        private static readonly object StaticScopeInfosLock = new object();
        private static readonly Dictionary<IScope, ScopeInfo> StaticScopeInfos = new Dictionary<IScope, ScopeInfo>();

        public IEnumerable<ScopeInfo> ScopeInfos
        {
            get
            {
                lock (StaticScopeInfosLock)
                {
                    return StaticScopeInfos.Values.ToArray(); // capture in an array
                }
            }
        }

        public ScopeInfo GetScopeInfo(IScope scope)
        {
            lock (StaticScopeInfosLock)
            {
                ScopeInfo scopeInfo;
                return StaticScopeInfos.TryGetValue(scope, out scopeInfo) ? scopeInfo : null;
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
                if (StaticScopeInfos.ContainsKey(scope)) throw new Exception("oops: already registered.");
                _logger.Debug<ScopeProvider>("Register " + scope.InstanceId.ToString("N").Substring(0, 8));
                StaticScopeInfos[scope] = new ScopeInfo(scope, Environment.StackTrace);
            }
        }

        // register that a scope is in a 'context'
        // 'context' that contains the scope (null, "http" or "call")
        public static void RegisterContext(IScope scope, string context)
        {
            lock (StaticScopeInfosLock)
            {
                ScopeInfo info;
                if (StaticScopeInfos.TryGetValue(scope, out info) == false) info = null;
                if (info == null)
                {
                    if (context == null) return;
                    throw new Exception("oops: unregistered scope.");
                }
                var sb = new StringBuilder();
                var s = scope;
                while (s != null)
                {
                    if (sb.Length > 0) sb.Append(" < ");
                    sb.Append(s.InstanceId.ToString("N").Substring(0, 8));
                    var ss = s as Scope;
                    s = ss?.ParentScope;
                }
                Current.Logger.Debug<ScopeProvider>("Register " + (context ?? "null") + " context " + sb);
                if (context == null) info.NullStack = Environment.StackTrace;
                //Current.Logger.Debug<ScopeProvider>("At:\r\n" + Head(Environment.StackTrace, 16));
                info.Context = context;
            }
        }

        private static string Head(string s, int count)
        {
            var pos = 0;
            var i = 0;
            while (i < count && pos >= 0)
            {
                pos = s.IndexOf("\r\n", pos + 1, StringComparison.OrdinalIgnoreCase);
                i++;
            }
            if (pos < 0) return s;
            return s.Substring(0, pos);
        }

        public void Disposed(IScope scope)
        {
            lock (StaticScopeInfosLock)
            {
                if (StaticScopeInfos.ContainsKey(scope))
                {
                    // enable this by default
                    //Console.WriteLine("unregister " + scope.InstanceId.ToString("N").Substring(0, 8));
                    StaticScopeInfos.Remove(scope);
                    _logger.Debug<ScopeProvider>("Remove " + scope.InstanceId.ToString("N").Substring(0, 8));

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

        public IScope Scope { get; } // the scope itself

        // the scope's parent identifier
        public Guid Parent => ((Scope) Scope).ParentScope == null ? Guid.Empty : ((Scope) Scope).ParentScope.InstanceId;

        public DateTime Created { get; } // the date time the scope was created
        public bool Disposed { get; set; } // whether the scope has been disposed already
        public string Context { get; set; } // the current 'context' that contains the scope (null, "http" or "lcc")

        public string CtorStack { get; } // the stacktrace of the scope ctor
        public string DisposedStack { get; set; } // the stacktrace when disposed
        public string NullStack { get; set; } // the stacktrace when the 'context' that contains the scope went null
    }
#endif
}
