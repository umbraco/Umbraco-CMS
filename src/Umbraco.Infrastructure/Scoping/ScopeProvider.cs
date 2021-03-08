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
using System.Collections.Generic;
using System.Collections.Concurrent;

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
        }

        public IUmbracoDatabaseFactory DatabaseFactory { get; }

        public ISqlContext SqlContext => DatabaseFactory.SqlContext;

        #region Context

        private static T GetCallContextObject<T>(string key)
            where T : class, IInstanceIdentifiable
        {
            ConcurrentStack<T> stack = CallContext<ConcurrentStack<T>>.GetData(key);
            if (stack == null || !stack.TryPeek(out T peek))
            {
                return null;
            }

            return peek;
        }

        internal void MoveHttpContextToCallContext<T>(string key)
            where T : class, IInstanceIdentifiable
        {
            var source = (ConcurrentStack<T>)_requestCache.Get(key);
            ConcurrentStack<T> stack = CallContext<ConcurrentStack<T>>.GetData(key);
            MoveContexts<T>(key, source, stack, (k, v) => CallContext<ConcurrentStack<T>>.SetData(k, v));
        }

        internal void MoveCallContextToHttpContext<T>(string key)
            where T : class, IInstanceIdentifiable
        {
            ConcurrentStack<T> source = CallContext<ConcurrentStack<T>>.GetData(key);
            var stack = (ConcurrentStack<T>)_requestCache.Get(key);
            MoveContexts<T>(key, source, stack, (k, v) => _requestCache.Set(k, v));
        }

        private void MoveContexts<T>(string key, ConcurrentStack<T> source, ConcurrentStack<T> stack, Action<string, ConcurrentStack<T>> setter)
            where T : class, IInstanceIdentifiable
        {
            if (source == null)
            {
                return;
            }

            if (stack != null)
            {
                stack.Clear();
            }
            else
            {
                // TODO: This isn't going to copy it back up the execution context chain
                stack = new ConcurrentStack<T>();
                setter(key, stack);
            }

            var arr = new T[source.Count];
            source.CopyTo(arr, 0);
            Array.Reverse(arr);
            foreach (T a in arr)
            {
                stack.Push(a);
            }

            source.Clear();
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

            ConcurrentStack<T> stack = CallContext<ConcurrentStack<T>>.GetData(key);

            if (value == null)
            {
                if (stack != null)
                {
                    stack.TryPop(out _);
                }
            }
            else
            {

#if DEBUG_SCOPES
                logger.LogDebug("AddObject " + value.InstanceId.ToString("N").Substring(0, 8));
#endif
                if (stack == null)
                {
                    stack = new ConcurrentStack<T>();
                }
                stack.Push(value);
                CallContext<ConcurrentStack<T>>.SetData(key, stack);
            }
        }


        private T GetHttpContextObject<T>(string key, bool required = true)
            where T : class
        {
            if (!_requestCache.IsAvailable && required)
            {
                throw new Exception("Request cache is unavailable.");
            }

            var stack = (ConcurrentStack<T>)_requestCache.Get(key);
            return stack != null && stack.TryPeek(out T peek) ? peek : null;
        }

        private bool SetHttpContextObject<T>(string key, T value, bool required = true)
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
            var stack = (ConcurrentStack<T>)_requestCache.Get(key);

            if (value == null)
            {
                if (stack != null)
                {
                    stack.TryPop(out _);
                }
            }
            else
            {
                if (stack == null)
                {
                    stack = new ConcurrentStack<T>();
                }
                stack.Push(value);
                _requestCache.Set(key, stack);
            }

            return true;
        }

        #endregion

        #region Ambient Context

        internal static readonly string ContextItemKey = typeof(ScopeProvider).FullName;

        /// <summary>
        /// Get the Ambient (Current) <see cref="IScopeContext"/> for the current execution context.
        /// </summary>
        /// <remarks>
        /// The current execution context may be request based (HttpContext) or on a background thread (AsyncLocal)
        /// </remarks>
        public IScopeContext AmbientContext
        {
            get
            {
                // try http context, fallback onto call context
                IScopeContext value = GetHttpContextObject<IScopeContext>(ContextItemKey, false);
                return value ?? GetCallContextObject<IScopeContext>(ContextItemKey);
            }
        }

        #endregion

        #region Ambient Scope

        internal static readonly string ScopeItemKey = typeof(Scope).FullName;
        
        IScope IScopeAccessor.AmbientScope => AmbientScope;

        /// <summary>
        /// Get or set the Ambient (Current) <see cref="Scope"/> for the current execution context.
        /// </summary>
        /// <remarks>
        /// The current execution context may be request based (HttpContext) or on a background thread (AsyncLocal)
        /// </remarks>
        public Scope AmbientScope => (Scope)GetHttpContextObject<IScope>(ScopeItemKey, false) ?? (Scope)GetCallContextObject<IScope>(ScopeItemKey);

        public void PopAmbientScope(Scope scope)
        {
            // pop the stack from all contexts
            SetHttpContextObject<IScope>(ScopeItemKey, null, false);
            SetCallContextObject<IScope>(ScopeItemKey, null, _logger);

            // We need to move the stack to a different context if the parent scope
            // is flagged with a different CallContext flag. This is required
            // if creating a child scope with callContext: true (thus forcing CallContext)
            // when there is actually a current HttpContext available.
            // It's weird but is required for Deploy somehow.
            bool parentScopeCallContext = (scope.ParentScope?.CallContext ?? false);
            if (scope.CallContext && !parentScopeCallContext)
            {
                MoveCallContextToHttpContext<IScope>(ScopeItemKey);
                MoveCallContextToHttpContext<IScopeContext>(ContextItemKey);
            }
            else if (!scope.CallContext && parentScopeCallContext)
            {
                MoveHttpContextToCallContext<IScope>(ScopeItemKey);
                MoveHttpContextToCallContext<IScopeContext>(ContextItemKey);
            } 
        }

        #endregion

        public void PushAmbientScope(Scope scope)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (scope.CallContext != false || !SetHttpContextObject<IScope>(ScopeItemKey, scope, false))
            {
                // In this case, always ensure that the HttpContext items
                // is transfered to CallContext and then cleared since we
                // may be migrating context with the callContext = true flag.
                // This is a weird case when forcing callContext when HttpContext
                // is available. Required by Deploy.

                if (_requestCache.IsAvailable)
                {
                    MoveHttpContextToCallContext<IScope>(ScopeItemKey);
                    MoveHttpContextToCallContext<IScopeContext>(ContextItemKey);
                }

                SetCallContextObject<IScope>(ScopeItemKey, scope, _logger);
            }
        }

        public void PushAmbientScopeContext(IScopeContext scopeContext)
        {
            if (scopeContext is null)
            {
                throw new ArgumentNullException(nameof(scopeContext));
            }

            SetHttpContextObject<IScopeContext>(ContextItemKey, scopeContext, false);
            SetCallContextObject<IScopeContext>(ContextItemKey, scopeContext, _logger);
        }

        public void PopAmbientScopeContext()
        {
            // pop stack from all contexts
            SetHttpContextObject<IScopeContext>(ContextItemKey, null, false);
            SetCallContextObject<IScopeContext>(ContextItemKey, null, _logger);
        }

        /// <inheritdoc />
        public IScope CreateDetachedScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null)
            => new Scope(this, _coreDebugSettings, _mediaFileSystem, _loggerFactory.CreateLogger<Scope>(), _fileSystems, true, null, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems);

        /// <inheritdoc />
        public void AttachScope(IScope other, bool callContext = false)
        {
            // IScopeProvider.AttachScope works with an IScope
            // but here we can only deal with our own Scope class
            if (other is not Scope otherScope)
            {
                throw new ArgumentException("Not a Scope instance.");
            }

            if (otherScope.Detachable == false)
            {
                throw new ArgumentException("Not a detachable scope.");
            }

            if (otherScope.Attached)
            {
                throw new InvalidOperationException("Already attached.");
            }

            otherScope.Attached = true;
            otherScope.OrigScope = AmbientScope;
            otherScope.OrigContext = AmbientContext;
            otherScope.CallContext = callContext;

            PushAmbientScopeContext(otherScope.Context);
            PushAmbientScope(otherScope);
        }

        /// <inheritdoc />
        public IScope DetachScope()
        {
            Scope ambientScope = AmbientScope;
            if (ambientScope == null)
            {
                throw new InvalidOperationException("There is no ambient scope.");
            }

            if (ambientScope.Detachable == false)
            {
                throw new InvalidOperationException("Ambient scope is not detachable.");
            }

            PopAmbientScope(ambientScope);
            PopAmbientScopeContext();

            Scope originalScope = AmbientScope;
            if (originalScope != ambientScope.OrigScope)
            {
                throw new InvalidOperationException($"The detatched scope ({ambientScope.GetDebugInfo()}) does not match the original ({originalScope.GetDebugInfo()})");
            }
            IScopeContext originalScopeContext = AmbientContext;
            if (originalScopeContext != ambientScope.OrigContext)
            {
                throw new InvalidOperationException($"The detatched scope context does not match the original");
            }
            
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
                PushAmbientScope(scope);
                if (newContext != null)
                {
                    PushAmbientScopeContext(newContext);
                }                 
                return scope;
            }

            var nested = new Scope(this, _coreDebugSettings, _mediaFileSystem, _loggerFactory.CreateLogger<Scope>(), _fileSystems, ambientScope, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext, autoComplete);
            PushAmbientScope(nested);
            return nested;
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
        }

        public IScope Scope { get; } // the scope itself

        // the scope's parent identifier
        public Guid Parent => ((Scope)Scope).ParentScope == null ? Guid.Empty : ((Scope)Scope).ParentScope.InstanceId;

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
                .Append("Parent Id: ")
                .AppendLine(Parent.ToString())
                .Append("Created Thread Id: ")
                .AppendLine(Scope.CreatedThreadId.ToInvariantString())
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
