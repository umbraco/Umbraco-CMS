using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using CoreDebugSettings = Umbraco.Cms.Core.Configuration.Models.CoreDebugSettings;

#if DEBUG_SCOPES
using System.Linq;
using System.Text;
#endif

namespace Umbraco.Cms.Infrastructure.Scoping
{
    /// <summary>
    /// Implements <see cref="IScopeProvider"/>.
    /// </summary>
    internal class ScopeProvider :
        ICoreScopeProvider,
        IScopeProvider,
        Core.Scoping.IScopeProvider,
        IScopeAccessor // TODO: No need to implement this here but literally hundreds of our tests cast ScopeProvider to ScopeAccessor
    {
        private readonly ILogger<ScopeProvider> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly IAmbientScopeStack _ambientScopeStack;
        private readonly IAmbientScopeContextStack _ambientContextStack;

        private readonly FileSystems _fileSystems;
        private CoreDebugSettings _coreDebugSettings;
        private readonly MediaFileManager _mediaFileManager;

        public ScopeProvider(
            IAmbientScopeStack ambientScopeStack,
            IAmbientScopeContextStack ambientContextStack,
            IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
            IUmbracoDatabaseFactory databaseFactory,
            FileSystems fileSystems,
            IOptionsMonitor<CoreDebugSettings> coreDebugSettings,
            MediaFileManager mediaFileManager,
            ILoggerFactory loggerFactory,
            IEventAggregator eventAggregator)
        {
            DistributedLockingMechanismFactory = distributedLockingMechanismFactory;
            DatabaseFactory = databaseFactory;
            _ambientScopeStack = ambientScopeStack;
            _ambientContextStack = ambientContextStack;
            _fileSystems = fileSystems;
            _coreDebugSettings = coreDebugSettings.CurrentValue;
            _mediaFileManager = mediaFileManager;
            _logger = loggerFactory.CreateLogger<ScopeProvider>();
            _loggerFactory = loggerFactory;
            _eventAggregator = eventAggregator;
            // take control of the FileSystems
            _fileSystems.IsScoped = () => AmbientScope != null && AmbientScope.ScopedFileSystems;

            coreDebugSettings.OnChange(x => _coreDebugSettings = x);
        }

        [Obsolete("Please use an alternative constructor. This constructor is due for removal in v12.")]
        public ScopeProvider(
            IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
            IUmbracoDatabaseFactory databaseFactory,
            FileSystems fileSystems,
            IOptionsMonitor<CoreDebugSettings> coreDebugSettings,
            MediaFileManager mediaFileManager,
            ILoggerFactory loggerFactory,
            IRequestCache requestCache,
            IEventAggregator eventAggregator)
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IAmbientScopeStack>(),
            StaticServiceProvider.Instance.GetRequiredService<IAmbientScopeContextStack>(),
            distributedLockingMechanismFactory,
            databaseFactory,
            fileSystems,
            coreDebugSettings,
            mediaFileManager,
            loggerFactory,
            eventAggregator)
        {
        }

        public IDistributedLockingMechanismFactory DistributedLockingMechanismFactory { get; }

        public IUmbracoDatabaseFactory DatabaseFactory { get; }

        public ISqlContext SqlContext => DatabaseFactory.SqlContext;


        #region Ambient Context

        /// <summary>
        /// Get the Ambient (Current) <see cref="IScopeContext"/> for the current execution context.
        /// </summary>
        /// <remarks>
        /// The current execution context may be request based (HttpContext) or on a background thread (AsyncLocal)
        /// </remarks>
        public IScopeContext? AmbientContext => _ambientContextStack.AmbientContext;

        #endregion

        #region Ambient Scope

        /// <summary>
        /// Gets or set the Ambient (Current) <see cref="Scope"/> for the current execution context.
        /// </summary>
        /// <remarks>
        /// The current execution context may be request based (HttpContext) or on a background thread (AsyncLocal)
        /// </remarks>
        public Scope? AmbientScope => (Scope?)_ambientScopeStack.AmbientScope;

        IScope? IScopeAccessor.AmbientScope => _ambientScopeStack.AmbientScope;

        public void PopAmbientScope() => _ambientScopeStack.Pop();

        #endregion

        public void PushAmbientScope(Scope scope)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            _ambientScopeStack.Push(scope);
        }

        public void PushAmbientScopeContext(IScopeContext? scopeContext)
        {
            if (scopeContext is null)
            {
                throw new ArgumentNullException(nameof(scopeContext));
            }
            _ambientContextStack.Push(scopeContext);
        }

        public void PopAmbientScopeContext() => _ambientContextStack.Pop();

        /// <inheritdoc />
        public IScope CreateDetachedScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher? eventDispatcher = null,
            IScopedNotificationPublisher? scopedNotificationPublisher = null,
            bool? scopeFileSystems = null)
            => new Scope(this, _coreDebugSettings, _mediaFileManager, _eventAggregator, _loggerFactory.CreateLogger<Scope>(), _fileSystems, true, null, isolationLevel, repositoryCacheMode, eventDispatcher, scopedNotificationPublisher, scopeFileSystems);

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

            PopAmbientScope();
            PopAmbientScopeContext();

            Scope? originalScope = AmbientScope;
            if (originalScope != ambientScope.OrigScope)
            {
                throw new InvalidOperationException($"The detatched scope ({ambientScope.GetDebugInfo()}) does not match the original ({originalScope?.GetDebugInfo()})");
            }
            IScopeContext? originalScopeContext = AmbientContext;
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
                var scope = new Scope(this, _coreDebugSettings, _mediaFileManager, _eventAggregator, _loggerFactory.CreateLogger<Scope>(), _fileSystems, false, newContext, isolationLevel, repositoryCacheMode, eventDispatcher, notificationPublisher, scopeFileSystems, callContext, autoComplete);

                // assign only if scope creation did not throw!
                PushAmbientScope(scope);
                if (newContext != null)
                {
                    PushAmbientScopeContext(newContext);
                }
                return scope;
            }

            var nested = new Scope(this, _coreDebugSettings, _mediaFileManager, _eventAggregator, _loggerFactory.CreateLogger<Scope>(), _fileSystems, ambientScope, isolationLevel, repositoryCacheMode, eventDispatcher, notificationPublisher, scopeFileSystems, callContext, autoComplete);
            PushAmbientScope(nested);
            return nested;
        }

        /// <inheritdoc />
        public IScopeContext? Context => AmbientContext;

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
        /// <inheritdoc />
        Cms.Core.Scoping.IScope Cms.Core.Scoping.IScopeProvider.CreateScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher? eventDispatcher = null,
            IScopedNotificationPublisher? notificationPublisher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false) =>
            (Cms.Core.Scoping.IScope) CreateScope(
                isolationLevel,
                repositoryCacheMode,
                eventDispatcher,
                notificationPublisher,
                scopeFileSystems,
                callContext,
                autoComplete);

        /// <inheritdoc />
        Core.Scoping.IScope Core.Scoping.IScopeProvider.CreateDetachedScope(IsolationLevel isolationLevel,
            RepositoryCacheMode repositoryCacheMode, IEventDispatcher? eventDispatcher,
            IScopedNotificationPublisher? scopedNotificationPublisher, bool? scopeFileSystems) =>
            (Core.Scoping.IScope)CreateDetachedScope(
                isolationLevel,
                repositoryCacheMode,
                eventDispatcher,
                scopedNotificationPublisher,
                scopeFileSystems);

        /// <inheritdoc />
        void Core.Scoping.IScopeProvider.AttachScope(Core.Scoping.IScope scope, bool callContext) =>
            AttachScope(scope, callContext);

        /// <inheritdoc />
        Core.Scoping.IScope Core.Scoping.IScopeProvider.DetachScope() =>
            (Core.Scoping.IScope)DetachScope();
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
