using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class GlobalSettingsBuilder : GlobalSettingsBuilder<object>
    {
        public GlobalSettingsBuilder() : base(null)
        {
        }
    }

    public class GlobalSettingsBuilder<TParent> : ChildBuilderBase<TParent, GlobalSettings>
    {
        private string _configurationStatus;
        private string _databaseFactoryServerVersion;
        private string _defaultUiLanguage;
        private bool? _disableElectionForSingleServer;
        private bool? _hideTopLevelNodeFromPath;
        private bool? _installEmptyDatabase;
        private bool? _installMissingDatabase;
        private string _path;
        private string _registerType;
        private string _reservedPaths;
        private string _reservedUrls;
        private int? _timeOutInMinutes;
        private string _umbracoCssPath;
        private string _umbracoMediaPath;
        private string _umbracoScriptsPath;
        private string _mainDomLock;
        private string _noNodesViewPath;
        private bool? _useHttps;
        private int? _versionCheckPeriod;
        private readonly SmtpSettingsBuilder<GlobalSettingsBuilder<TParent>> _smtpSettingsBuilder;

        public GlobalSettingsBuilder(TParent parentBuilder) : base(parentBuilder)
        {
            _smtpSettingsBuilder = new SmtpSettingsBuilder<GlobalSettingsBuilder<TParent>>(this);
        }

        public SmtpSettingsBuilder<GlobalSettingsBuilder<TParent>> AddSmtpSettings() => _smtpSettingsBuilder;

        public GlobalSettingsBuilder<TParent> WithConfigurationStatus(string configurationStatus)
        {
            _configurationStatus = configurationStatus;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithDatabaseFactoryServerVersion(string databaseFactoryServerVersion)
        {
            _databaseFactoryServerVersion = databaseFactoryServerVersion;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithDefaultUiLanguage(string defaultUiLanguage)
        {
            _defaultUiLanguage = defaultUiLanguage;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithDisableElectionForSingleServer(bool disableElectionForSingleServer)
        {
            _disableElectionForSingleServer = disableElectionForSingleServer;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithHideTopLevelNodeFromPath(bool hideTopLevelNodeFromPath)
        {
            _hideTopLevelNodeFromPath = hideTopLevelNodeFromPath;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithInstallEmptyDatabase(bool installEmptyDatabase)
        {
            _installEmptyDatabase = installEmptyDatabase;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithInstallMissingDatabase(bool installMissingDatabase)
        {
            _installMissingDatabase = installMissingDatabase;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithPath(string path)
        {
            _path = path;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithRegisterType(string registerType)
        {
            _registerType = registerType;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithReservedPaths(string reservedPaths)
        {
            _reservedPaths = reservedPaths;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithReservedUrls(string reservedUrls)
        {
            _reservedUrls = reservedUrls;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithUseHttps(bool useHttps)
        {
            _useHttps = useHttps;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithUmbracoCssPath(string umbracoCssPath)
        {
            _umbracoCssPath = umbracoCssPath;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithUmbracoMediaPath(string umbracoMediaPath)
        {
            _umbracoMediaPath = umbracoMediaPath;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithUmbracoScriptsPath(string umbracoScriptsPath)
        {
            _umbracoScriptsPath = umbracoScriptsPath;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithMainDomLock(string mainDomLock)
        {
            _mainDomLock = mainDomLock;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithNoNodesViewPath(string noNodesViewPath)
        {
            _noNodesViewPath = noNodesViewPath;
            return this;
        }
        public GlobalSettingsBuilder<TParent> WithVersionCheckPeriod(int versionCheckPeriod)
        {
            _versionCheckPeriod = versionCheckPeriod;
            return this;
        }

        public GlobalSettingsBuilder<TParent> WithTimeOutInMinutes(int timeOutInMinutes)
        {
            _timeOutInMinutes = timeOutInMinutes;
            return this;
        }

        public override GlobalSettings Build()
        {
            var configurationStatus = _configurationStatus ?? "9.0.0";
            var databaseFactoryServerVersion = _databaseFactoryServerVersion ?? null;
            var defaultUiLanguage = _defaultUiLanguage ?? "en";
            var disableElectionForSingleServer = _disableElectionForSingleServer ?? false;
            var hideTopLevelNodeFromPath = _hideTopLevelNodeFromPath ?? false;
            var installEmptyDatabase = _installEmptyDatabase ?? false;
            var installMissingDatabase = _installMissingDatabase ?? false;
            var registerType = _registerType ?? null;
            var reservedPaths = _reservedPaths ?? GlobalSettings.StaticReservedPaths;
            var reservedUrls = _reservedUrls ?? GlobalSettings.StaticReservedUrls;
            var path = _path ?? "~/umbraco";
            var useHttps = _useHttps ?? false;
            var umbracoCssPath = _umbracoCssPath ?? "~/css";
            var umbracoMediaPath = _umbracoMediaPath ?? "~/media";
            var umbracoScriptsPath = _umbracoScriptsPath ?? "~/scripts";
            var versionCheckPeriod = _versionCheckPeriod ?? 0;
            var timeOutInMinutes = _timeOutInMinutes ?? 20;
            var smtpSettings = _smtpSettingsBuilder.Build();
            var mainDomLock = _mainDomLock ?? string.Empty;
            var noNodesViewPath = _noNodesViewPath ?? "~/config/splashes/NoNodes.cshtml";

            return new GlobalSettings
            {
                ConfigurationStatus = configurationStatus,
                DatabaseFactoryServerVersion = databaseFactoryServerVersion,
                DefaultUILanguage = defaultUiLanguage,
                DisableElectionForSingleServer = disableElectionForSingleServer,
                HideTopLevelNodeFromPath = hideTopLevelNodeFromPath,
                InstallEmptyDatabase = installEmptyDatabase,
                InstallMissingDatabase = installMissingDatabase,
                RegisterType = registerType,
                ReservedPaths = reservedPaths,
                ReservedUrls = reservedUrls,
                Path = path,
                UseHttps = useHttps,
                UmbracoCssPath = umbracoCssPath,
                UmbracoMediaPath = umbracoMediaPath,
                UmbracoScriptsPath = umbracoScriptsPath,
                VersionCheckPeriod = versionCheckPeriod,
                TimeOutInMinutes = timeOutInMinutes,
                Smtp = smtpSettings,
                MainDomLock = mainDomLock,
                NoNodesViewPath = noNodesViewPath,
            };
        }
    }
}
