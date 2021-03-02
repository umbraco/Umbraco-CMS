using System;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Infrastructure.Persistence;
using CoreDebugSettings = Umbraco.Cms.Core.Configuration.Models.CoreDebugSettings;
using Umbraco.Extensions;

#if DEBUG_SCOPES
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endif

namespace Umbraco.Cms.Core.Scoping
{
    /// <summary>
    /// Implements <see cref="IScopeProvider"/>.
    /// </summary>
    internal class ScopeProvider : IScopeProvider, IScopeAccessor
    {
        private readonly ILogger<ScopeProvider> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IRequestCache _requestCache;
        private readonly FileSystems _fileSystems;
        private readonly CoreDebugSettings _coreDebugSettings;
        private readonly IMediaFileSystem _mediaFileSystem;

        public ScopeProvider(IUmbracoDatabaseFactory databaseFactory, FileSystems fileSystems, IOptions<CoreDebugSettings> coreDebugSettings, IMediaFileSystem mediaFileSystem, ILogger<ScopeProvider> logger, ILoggerFactory loggerFactory, IRequestCache requestCache)
        {
            DatabaseFactory = databaseFactory;
            _fileSystems = fileSystems;
            _coreDebugSettings = coreDebugSettings.Value;
            _mediaFileSystem = mediaFileSystem;
            _logger = logger;
            _loggerFactory = loggerFactory;
            _requestCache = requestCache;
            // take control of the FileSystems
            _fileSystems.IsScoped = () => AmbientScope != null && AmbientScope.ScopedFileSystems;

            _scopeReference = new ScopeReference(this);
        }

        public IUmbracoDatabaseFactory DatabaseFactory { get; }

        public ISqlContext SqlContext => DatabaseFactory.SqlContext;

        #region Context

        private static T GetCallContextObject<T>(string key)
            where T : class, IInstanceIdentifiable
        {
            T obj = CallContext<T>.GetData(key);
            if (obj == default(T))
            {
                return null;
            }

            return obj;
        }

        private static void SetCallContextObject<T>(string key, T value, ILogger<ScopeProvider> logger)
            where T : class, IInstanceIdentifiable
        {
#if DEBUG_SCOPES
            // manage the 'context' that contains the scope (null, "http" or "call")
            // only for scopes of course!
            if (key == ScopeItemKey)
            {
                // first, null-register the existing value
                IScope ambientScope = CallContext<IScope>.GetData(ScopeItemKey);

                if (ambientScope != null)
                {
                    RegisterContext(ambientScope, logger, null);
                }

                // then register the new value
                if (value is IScope scope)
                {
                    RegisterContext(scope, logger, "call");
                }
            }
#endif
            if (value == null)
            {
                T obj = CallContext<T>.GetData(key);
                CallContext<T>.SetData(key, default); // aka remove
                if (obj == null)
                {
                    return;
                }
            }
            else
            {

#if DEBUG_SCOPES
                logger.LogDebug("AddObject " + value.InstanceId.ToString("N").Substring(0, 8));
#endif

                CallContext<T>.SetData(key, value);
            }
        }


        private T GetHttpContextObject<T>(string key, bool required = true)
            where T : class
        {

            if (!_requestCache.IsAvailable && required)
            {
                throw new Exception("Request cache is unavailable.");
            }

            return (T)_requestCache.Get(key);
        }

        private bool SetHttpContextObject(string key, object value, bool required = true)
        {
            if (!_requestCache.IsAvailable)
            {
                if (required)
                {
                    throw new Exception("Request cache is unavailable.");
                }

                return false;
            }

#if DEBUG_SCOPES
            // manage the 'context' that contains the scope (null, "http" or "call")
            // only for scopes of course!
            if (key == ScopeItemKey)
            {
                // first, null-register the existing value
                var ambientScope = (IScope)_requestCache.Get(ScopeItemKey);
                if (ambientScope != null)
                {
                    RegisterContext(ambientScope, _logger, null);
                }

                // then register the new value
                if (value is IScope scope)
                {
                    RegisterContext(scope, _logger, "http");
                }
            }
#endif
            if (value == null)
            {
                _requestCache.Remove(key);
            }
            else
            {
                _requestCache.Set(key, value);
            }

            return true;
        }

        #endregion

        #region Ambient Context

        internal const string ContextItemKey = "Umbraco.Core.Scoping.ScopeContext";

        public IScopeContext AmbientContext
        {
            get
            {
                // try http context, fallback onto call context
                IScopeContext value = GetHttpContextObject<IScopeContext>(ContextItemKey, false);
                return value ?? GetCallContextObject<IScopeContext>(ContextItemKey);
            }
            set
            {
                // clear both
                SetHttpContextObject(ContextItemKey, null, false);
                SetCallContextObject<IScopeContext>(ContextItemKey, null, _logger);
                if (value == null)
                {
                    return;
                }

                // set http/call context
                if (SetHttpContextObject(ContextItemKey, value, false) == false)
                {
                    SetCallContextObject<IScopeContext>(ContextItemKey, value, _logger);
                }
            }
        }

        #endregion

        #region Ambient Scope

        internal static readonly string ScopeItemKey = typeof(Scope).FullName;
        internal static readonly string ScopeRefItemKey = typeof(ScopeReference).FullName;

        // only 1 instance which can be disposed and disposed again
        private readonly ScopeReference _scopeReference;

        IScope IScopeAccessor.AmbientScope => AmbientScope;

        // null if there is none
        public Scope AmbientScope
        {
            // try http context, fallback onto call context
            // we are casting here because we know its a concrete type
            get => (Scope)GetHttpContextObject<IScope>(ScopeItemKey, false)
                   ?? (Scope)GetCallContextObject<IScope>(ScopeItemKey);
            set
            {
                // clear both
                SetHttpContextObject(ScopeItemKey, null, false);
                SetHttpContextObject(ScopeRefItemKey, null, false);
                SetCallContextObject<IScope>(ScopeItemKey, null, _logger);
                if (value == null)
                {
                    return;
                }

                // set http/call context
                if (value.CallContext == false && SetHttpContextObject(ScopeItemKey, value, false))
                {
                    SetHttpContextObject(ScopeRefItemKey, _scopeReference);
                }
                else
                {
                    SetCallContextObject<IScope>(ScopeItemKey, value, _logger);
                }
            }
        }

        #endregion

        public void SetAmbient(Scope scope, IScopeContext context = null)
        {
            // clear all
            SetHttpContextObject(ScopeItemKey, null, false);
            SetHttpContextObject(ScopeRefItemKey, null, false);
            SetCallContextObject<IScope>(ScopeItemKey, null, _logger);
            SetHttpContextObject(ContextItemKey, null, false);
            SetCallContextObject<IScopeContext>(ContextItemKey, null, _logger);
            if (scope == null)
            {
                if (context != null)
                {
                    throw new ArgumentException("Must be null if scope is null.", nameof(context));
                }

                return;
            }

            if (scope.CallContext == false && SetHttpContextObject(ScopeItemKey, scope, false))
            {
                SetHttpContextObject(ScopeRefItemKey, _scopeReference);
                SetHttpContextObject(ContextItemKey, context);
            }
            else
            {
                SetCallContextObject<IScope>(ScopeItemKey, scope, _logger);
                SetCallContextObject<IScopeContext>(ContextItemKey, context, _logger);
            }
        }

        /// <inheritdoc />
        public IScope CreateDetachedScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null)
        {
            return new Scope(this, _coreDebugSettings, _mediaFileSystem, _loggerFactory.CreateLogger<Scope>(), _fileSystems, true, null, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems);
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
            Scope ambientScope = AmbientScope;
            if (ambientScope == null)
            {
                IScopeContext ambientContext = AmbientContext;
                ScopeContext newContext = ambientContext == null ? new ScopeContext() : null;
                var scope = new Scope(this, _coreDebugSettings, _mediaFileSystem, _loggerFactory.CreateLogger<Scope>(), _fileSystems, false, newContext, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext, autoComplete);
                // assign only if scope creation did not throw!
                SetAmbient(scope, newContext ?? ambientContext);
                return scope;
            }

            var nested = new Scope(this, _coreDebugSettings, _mediaFileSystem, _loggerFactory.CreateLogger<Scope>(), _fileSystems, ambientScope, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext, autoComplete);
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
        private static readonly object s_staticScopeInfosLock = new object();
        private static readonly Dictionary<IScope, ScopeInfo> s_staticScopeInfos = new Dictionary<IScope, ScopeInfo>();

        public IEnumerable<ScopeInfo> ScopeInfos
        {
            get
            {
                lock (s_staticScopeInfosLock)
                {
                    return s_staticScopeInfos.Values.ToArray(); // capture in an array
                }
            }
        }

        public ScopeInfo GetScopeInfo(IScope scope)
        {
            lock (s_staticScopeInfosLock)
            {
                return s_staticScopeInfos.TryGetValue(scope, out ScopeInfo scopeInfo) ? scopeInfo : null;
            }
        }

        // register a scope and capture its ctor stacktrace
        public void RegisterScope(IScope scope)
        {
            lock (s_staticScopeInfosLock)
            {
                if (s_staticScopeInfos.ContainsKey(scope))
                {
                    throw new Exception("oops: already registered.");
                }

                _logger.LogDebug("Register " + scope.InstanceId.ToString("N").Substring(0, 8));
                s_staticScopeInfos[scope] = new ScopeInfo(scope, Environment.StackTrace);
            }
        }

        // register that a scope is in a 'context'
        // 'context' that contains the scope (null, "http" or "call")
        public static void RegisterContext(IScope scope, ILogger<ScopeProvider> logger, string context)
        {
            lock (s_staticScopeInfosLock)
            {
                if (s_staticScopeInfos.TryGetValue(scope, out ScopeInfo info) == false)
                {
                    info = null;
                }

                if (info == null)
                {
                    if (context == null)
                    {
                        return;
                    }

                    throw new Exception("oops: unregistered scope.");
                }
                var sb = new StringBuilder();
                IScope s = scope;
                while (s != null)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" < ");
                    }

                    sb.Append(s.InstanceId.ToString("N").Substring(0, 8));
                    var ss = s as Scope;
                    s = ss?.ParentScope;
                }

                logger?.LogTrace("Register " + (context ?? "null") + " context " + sb);

                if (context == null)
                {
                    info.NullStack = Environment.StackTrace;
                }

                logger?.LogTrace("At:\r\n" + Head(Environment.StackTrace, 16));

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

            if (pos < 0)
            {
                return s;
            }

            return s.Substring(0, pos);
        }

        public void Disposed(IScope scope)
        {
            lock (s_staticScopeInfosLock)
            {
                if (s_staticScopeInfos.ContainsKey(scope))
                {
                    // enable this by default
                    //Console.WriteLine("unregister " + scope.InstanceId.ToString("N").Substring(0, 8));
                    s_staticScopeInfos.Remove(scope);
                    _logger.LogDebug("Remove " + scope.InstanceId.ToString("N").Substring(0, 8));

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
            CreatedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public IScope Scope { get; } // the scope itself

        // the scope's parent identifier
        public Guid Parent => ((Scope)Scope).ParentScope == null ? Guid.Empty : ((Scope)Scope).ParentScope.InstanceId;

        public int CreatedThreadId { get; } // the thread id that created this scope
        public DateTime Created { get; } // the date time the scope was created
        public bool Disposed { get; set; } // whether the scope has been disposed already
        public string Context { get; set; } // the current 'context' that contains the scope (null, "http" or "lcc")

        public string CtorStack { get; } // the stacktrace of the scope ctor
        //public string DisposedStack { get; set; } // the stacktrace when disposed
        public string NullStack { get; set; } // the stacktrace when the 'context' that contains the scope went null

        public override string ToString() => new StringBuilder()
                .AppendLine("ScopeInfo:")
                .Append("Instance Id: ")
                .AppendLine(Scope.InstanceId.ToString())
                .Append("Instance Id: ")
                .AppendLine(Parent.ToString())
                .Append("Created Thread Id: ")
                .AppendLine(CreatedThreadId.ToInvariantString())
                .Append("Created At: ")
                .AppendLine(Created.ToString("O"))
                .Append("Disposed: ")
                .AppendLine(Disposed.ToString())
                .Append("CTOR stack: ")
                .AppendLine(CtorStack)
                .ToString();
    }
#endif
}
