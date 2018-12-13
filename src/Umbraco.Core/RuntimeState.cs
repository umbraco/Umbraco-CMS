using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the state of the Umbraco runtime.
    /// </summary>
    internal class RuntimeState : IRuntimeState
    {
        private readonly ILogger _logger;
        private readonly IUmbracoSettingsSection _settings;
        private readonly IGlobalSettings _globalSettings;
        private readonly HashSet<string> _applicationUrls = new HashSet<string>();
        private readonly Lazy<IMainDom> _mainDom;
        private readonly Lazy<IServerRegistrar> _serverRegistrar;
        private RuntimeLevel _level;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeState"/> class.
        /// </summary>
        public RuntimeState(ILogger logger, IUmbracoSettingsSection settings, IGlobalSettings globalSettings,
            Lazy<IMainDom> mainDom, Lazy<IServerRegistrar> serverRegistrar)
        {
            _logger = logger;
            _settings = settings;
            _globalSettings = globalSettings;
            _mainDom = mainDom;
            _serverRegistrar = serverRegistrar;
        }

        /// <summary>
        /// Gets the server registrar.
        /// </summary>
        /// <remarks>
        /// <para>This is NOT exposed in the interface.</para>
        /// </remarks>
        private IServerRegistrar ServerRegistrar => _serverRegistrar.Value;

        /// <summary>
        /// Gets the application MainDom.
        /// </summary>
        /// <remarks>
        /// <para>This is NOT exposed in the interface as MainDom is internal.</para>
        /// </remarks>
        public IMainDom MainDom => _mainDom.Value;

        /// <inheritdoc />
        public Version Version => UmbracoVersion.Current;

        /// <inheritdoc />
        public string VersionComment => UmbracoVersion.Comment;

        /// <inheritdoc />
        public SemVersion SemanticVersion => UmbracoVersion.SemanticVersion;

        /// <inheritdoc />
        public bool Debug { get; } = GlobalSettings.DebugMode;

        /// <inheritdoc />
        public bool IsMainDom => MainDom.IsMainDom;

        /// <inheritdoc />
        public ServerRole ServerRole => ServerRegistrar.GetCurrentServerRole();

        /// <inheritdoc />
        public Uri ApplicationUrl { get; private set; }

        /// <inheritdoc />
        public string ApplicationVirtualPath { get; } = HttpRuntime.AppDomainAppVirtualPath;

        /// <inheritdoc />
        public string CurrentMigrationState { get; internal set; }

        /// <inheritdoc />
        public string FinalMigrationState { get; internal set; }

        /// <inheritdoc />
        public RuntimeLevel Level
        {
            get => _level;
            internal set { _level = value; if (value == RuntimeLevel.Run) _runLevel.Set(); }
        }

        /// <summary>
        /// Ensures that the <see cref="ApplicationUrl"/> property has a value.
        /// </summary>
        /// <param name="request"></param>
        internal void EnsureApplicationUrl(HttpRequestBase request = null)
        {
            // see U4-10626 - in some cases we want to reset the application url
            // (this is a simplified version of what was in 7.x)
            // note: should this be optional? is it expensive?
            var url = request == null ? null : ApplicationUrlHelper.GetApplicationUrlFromCurrentRequest(request, _globalSettings);
            var change = url != null && !_applicationUrls.Contains(url);
            if (change)
            {
                _logger.Info(typeof(ApplicationUrlHelper), "New url {Url} detected, re-discovering application url.", url);
                _applicationUrls.Add(url);
            }

            if (ApplicationUrl != null && !change) return;
            ApplicationUrl = new Uri(ApplicationUrlHelper.GetApplicationUrl(_logger, _globalSettings, _settings, ServerRegistrar, request));
        }

        private readonly ManualResetEventSlim _runLevel = new ManualResetEventSlim(false);

        /// <summary>
        /// Waits for the runtime level to become RuntimeLevel.Run.
        /// </summary>
        /// <param name="timeout">A timeout.</param>
        /// <returns>True if the runtime level became RuntimeLevel.Run before the timeout, otherwise false.</returns>
        internal bool WaitForRunLevel(TimeSpan timeout)
        {
            return _runLevel.WaitHandle.WaitOne(timeout);
        }

        /// <inheritdoc />
        public BootFailedException BootFailedException { get; internal set; }
    }
}
