using System.Collections.Concurrent;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;
#if DEBUG_SCOPES
using System.Linq;
using System.Text;
#endif

namespace Umbraco.Cms.Infrastructure.Scoping
{
    /// <summary>
    ///     Implements <see cref="IScopeProvider" />.
    /// </summary>
    internal class ScopeProvider :
        ICoreScopeProvider,
        IScopeProvider,
        Core.Scoping.IScopeProvider,
        IScopeAccessor
    {
        private readonly ILogger<ScopeProvider> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IRequestCache _requestCache;
        private readonly FileSystems _fileSystems;
        private CoreDebugSettings _coreDebugSettings;
        private readonly MediaFileManager _mediaFileManager;
        private static readonly AsyncLocal<ConcurrentStack<IScope>> s_scopeStack = new();
        private static readonly AsyncLocal<ConcurrentStack<IScopeContext>> s_scopeContextStack = new();
        private static readonly string s_scopeItemKey = typeof(Scope).FullName!;
        private static readonly string s_contextItemKey = typeof(ScopeProvider).FullName!;
        private readonly IEventAggregator _eventAggregator;

        public ScopeProvider(
            IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
            IUmbracoDatabaseFactory databaseFactory,
            FileSystems fileSystems,
            IOptionsMonitor<CoreDebugSettings> coreDebugSettings,
            MediaFileManager mediaFileManager,
            ILoggerFactory loggerFactory,
            IRequestCache requestCache,
            IEventAggregator eventAggregator)
        {
            DistributedLockingMechanismFactory = distributedLockingMechanismFactory;
            DatabaseFactory = databaseFactory;
            _fileSystems = fileSystems;
            _coreDebugSettings = coreDebugSettings.CurrentValue;
            _mediaFileManager = mediaFileManager;
            _logger = loggerFactory.CreateLogger<ScopeProvider>();
            _loggerFactory = loggerFactory;
            _requestCache = requestCache;
            _eventAggregator = eventAggregator;
            // take control of the FileSystems
            _fileSystems.IsScoped = () => AmbientScope != null && AmbientScope.ScopedFileSystems;

            coreDebugSettings.OnChange(x => _coreDebugSettings = x);
        }

        public IDistributedLockingMechanismFactory DistributedLockingMechanismFactory { get; }

        public IUmbracoDatabaseFactory DatabaseFactory { get; }

        public ISqlContext SqlContext => DatabaseFactory.SqlContext;

        #region Context

        private void MoveHttpContextScopeToCallContext()
        {
            var source = (ConcurrentStack<IScope>?)_requestCache.Get(s_scopeItemKey);
            ConcurrentStack<IScope>? stack = s_scopeStack.Value;
            MoveContexts(s_scopeItemKey, source, stack, (_, v) => s_scopeStack.Value = v);
        }

        private void MoveHttpContextScopeContextToCallContext()
        {
            var source = (ConcurrentStack<IScopeContext>?)_requestCache.Get(s_contextItemKey);
            ConcurrentStack<IScopeContext>? stack = s_scopeContextStack.Value;
            MoveContexts(s_contextItemKey, source, stack, (_, v) => s_scopeContextStack.Value = v);
        }

        private void MoveCallContextScopeToHttpContext()
        {
            ConcurrentStack<IScope>? source = s_scopeStack.Value;
            var stack = (ConcurrentStack<IScope>?)_requestCache.Get(s_scopeItemKey);
            MoveContexts(s_scopeItemKey, source, stack, (k, v) => _requestCache.Set(k, v));
        }

        private void MoveCallContextScopeContextToHttpContext()
        {
            ConcurrentStack<IScopeContext>? source = s_scopeContextStack.Value;
            var stack = (ConcurrentStack<IScopeContext>?)_requestCache.Get(s_contextItemKey);
            MoveContexts(s_contextItemKey, source, stack, (k, v) => _requestCache.Set(k, v));
        }

        private void MoveContexts<T>(string key, ConcurrentStack<T>? source, ConcurrentStack<T>? stack,
            Action<string, ConcurrentStack<T>> setter)
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

        private void SetCallContextScope(IScope? value)
        {
            ConcurrentStack<IScope>? stack = s_scopeStack.Value;

#if DEBUG_SCOPES
            // first, null-register the existing value
            if (stack != null && stack.TryPeek(out IScope ambientScope))
            {
                RegisterContext(ambientScope, null);
            }

            // then register the new value
            if (value != null)
            {
                RegisterContext(value, "call");
            }
#endif

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
                _logger.LogDebug("AddObject " + value.InstanceId.ToString("N").Substring(0, 8));
#endif
                if (stack == null)
                {
                    stack = new ConcurrentStack<IScope>();
                }

                stack.Push(value);
                s_scopeStack.Value = stack;
            }
        }

        private void SetCallContextScopeContext(IScopeContext? value)
        {
            ConcurrentStack<IScopeContext>? stack = s_scopeContextStack.Value;

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
                    stack = new ConcurrentStack<IScopeContext>();
                }

                stack.Push(value);
                s_scopeContextStack.Value = stack;
            }
        }


        private T? GetHttpContextObject<T>(string key, bool required = true)
            where T : class
        {
            if (!_requestCache.IsAvailable && required)
            {
                throw new Exception("Request cache is unavailable.");
            }

            var stack = (ConcurrentStack<T>?)_requestCache.Get(key);
            return stack != null && stack.TryPeek(out T? peek) ? peek : null;
        }

        private bool SetHttpContextObject<T>(string key, T? value, bool required = true)
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
            if (key == s_scopeItemKey)
            {
                // first, null-register the existing value
                var ambientScope = (IScope)_requestCache.Get(s_scopeItemKey);
                if (ambientScope != null)
                {
                    RegisterContext(ambientScope, null);
                }

                // then register the new value
                if (value is IScope scope)
                {
                    RegisterContext(scope, "http");
                }
            }
#endif
            var stack = (ConcurrentStack<T>?)_requestCache.Get(key);

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

        /// <summary>
        ///     Get the Ambient (Current) <see cref="IScopeContext" /> for the current execution context.
        /// </summary>
        /// <remarks>
        ///     The current execution context may be request based (HttpContext) or on a background thread (AsyncLocal)
        /// </remarks>
        public IScopeContext? AmbientContext
        {
            get
            {
                // try http context, fallback onto call context
                IScopeContext? value = GetHttpContextObject<IScopeContext>(s_contextItemKey, false);
                if (value != null)
                {
                    return value;
                }

                ConcurrentStack<IScopeContext>? stack = s_scopeContextStack.Value;
                if (stack == null || !stack.TryPeek(out IScopeContext? peek))
                {
                    return null;
                }

                return peek;
            }
        }

        #endregion

        #region Ambient Scope

        IScope? IScopeAccessor.AmbientScope => AmbientScope;

        /// <summary>
        ///     Gets or set the Ambient (Current) <see cref="Scope" /> for the current execution context.
        /// </summary>
        /// <remarks>
        ///     The current execution context may be request based (HttpContext) or on a background thread (AsyncLocal)
        /// </remarks>
        public Scope? AmbientScope
        {
            get
            {
                // try http context, fallback onto call context
                IScope? value = GetHttpContextObject<IScope>(s_scopeItemKey, false);
                if (value != null)
                {
                    return (Scope)value;
                }

                ConcurrentStack<IScope>? stack = s_scopeStack.Value;
                if (stack == null || !stack.TryPeek(out IScope? peek))
                {
                    return null;
                }

                return (Scope)peek;
            }
        }

        public void PopAmbientScope(Scope? scope)
        {
            // pop the stack from all contexts
            SetHttpContextObject<IScope>(s_scopeItemKey, null, false);
            SetCallContextScope(null);

            // We need to move the stack to a different context if the parent scope
            // is flagged with a different CallContext flag. This is required
            // if creating a child scope with callContext: true (thus forcing CallContext)
            // when there is actually a current HttpContext available.
            // It's weird but is required for Deploy somehow.
            var parentScopeCallContext = scope?.ParentScope?.CallContext ?? false;
            if ((scope?.CallContext ?? false) && !parentScopeCallContext)
            {
                MoveCallContextScopeToHttpContext();
                MoveCallContextScopeContextToHttpContext();
            }
            else if ((!scope?.CallContext ?? false) && parentScopeCallContext)
            {
                MoveHttpContextScopeToCallContext();
                MoveHttpContextScopeContextToCallContext();
            }
        }

        #endregion

        public void PushAmbientScope(Scope scope)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (scope.CallContext || !SetHttpContextObject<IScope>(s_scopeItemKey, scope, false))
            {
                // In this case, always ensure that the HttpContext items
                // is transfered to CallContext and then cleared since we
                // may be migrating context with the callContext = true flag.
                // This is a weird case when forcing callContext when HttpContext
                // is available. Required by Deploy.

                if (_requestCache.IsAvailable)
                {
                    MoveHttpContextScopeToCallContext();
                    MoveHttpContextScopeContextToCallContext();
                }

                SetCallContextScope(scope);
            }
        }

        public void PushAmbientScopeContext(IScopeContext? scopeContext)
        {
            if (scopeContext is null)
            {
                throw new ArgumentNullException(nameof(scopeContext));
            }

            SetHttpContextObject(s_contextItemKey, scopeContext, false);
            SetCallContextScopeContext(scopeContext);
        }

        public void PopAmbientScopeContext()
        {
            // pop stack from all contexts
            SetHttpContextObject<IScopeContext>(s_contextItemKey, null, false);
            SetCallContextScopeContext(null);
        }

        /// <inheritdoc />
        public IScope CreateDetachedScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher? eventDispatcher = null,
            IScopedNotificationPublisher? scopedNotificationPublisher = null,
            bool? scopeFileSystems = null)
            => new Scope(this, _coreDebugSettings, _mediaFileManager, _eventAggregator,
                _loggerFactory.CreateLogger<Scope>(), _fileSystems, true, null, isolationLevel, repositoryCacheMode,
                eventDispatcher, scopedNotificationPublisher, scopeFileSystems);

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
            Scope? ambientScope = AmbientScope;
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

            Scope? originalScope = AmbientScope;
            if (originalScope != ambientScope.OrigScope)
            {
                throw new InvalidOperationException(
                    $"The detatched scope ({ambientScope.GetDebugInfo()}) does not match the original ({originalScope?.GetDebugInfo()})");
            }

            IScopeContext? originalScopeContext = AmbientContext;
            if (originalScopeContext != ambientScope.OrigContext)
            {
                throw new InvalidOperationException("The detatched scope context does not match the original");
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
            IEventDispatcher? eventDispatcher = null,
            IScopedNotificationPublisher? notificationPublisher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false)
        {
            Scope? ambientScope = AmbientScope;
            if (ambientScope == null)
            {
                IScopeContext? ambientContext = AmbientContext;
                ScopeContext? newContext = ambientContext == null ? new ScopeContext() : null;
                var scope = new Scope(this, _coreDebugSettings, _mediaFileManager, _eventAggregator,
                    _loggerFactory.CreateLogger<Scope>(), _fileSystems, false, newContext, isolationLevel,
                    repositoryCacheMode, eventDispatcher, notificationPublisher, scopeFileSystems, callContext,
                    autoComplete);

                // assign only if scope creation did not throw!
                PushAmbientScope(scope);
                if (newContext != null)
                {
                    PushAmbientScopeContext(newContext);
                }

                return scope;
            }

            var nested = new Scope(this, _coreDebugSettings, _mediaFileManager, _eventAggregator,
                _loggerFactory.CreateLogger<Scope>(), _fileSystems, ambientScope, isolationLevel, repositoryCacheMode,
                eventDispatcher, notificationPublisher, scopeFileSystems, callContext, autoComplete);
            PushAmbientScope(nested);
            return nested;
        }

        /// <inheritdoc />
        public IScopeContext? Context => AmbientContext;

        // for testing
        internal ConcurrentStack<IScope>? GetCallContextScopeValue() => s_scopeStack.Value;

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
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            lock (s_staticScopeInfosLock)
            {
                if (s_staticScopeInfos.ContainsKey(scope))
                {
                    throw new Exception("oops: already registered.");
                }

                _logger.LogDebug("Register {ScopeId} on Thread {ThreadId}", scope.InstanceId.ToString("N").Substring(0, 8), Thread.CurrentThread.ManagedThreadId);
                s_staticScopeInfos[scope] = new ScopeInfo(scope, Environment.StackTrace);
            }
        }

        // register that a scope is in a 'context'
        // 'context' that contains the scope (null, "http" or "call")
        public void RegisterContext(IScope scope, string context)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

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

                _logger.LogTrace("Register " + (context ?? "null") + " context " + sb);

                if (context == null)
                {
                    info.NullStack = Environment.StackTrace;
                }

                _logger.LogTrace("At:\r\n" + Head(Environment.StackTrace, 16));

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
