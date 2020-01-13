using Umbraco.Core.Configuration;

namespace Umbraco.Tests.Shared.Builders
{
    public class GlobalSettingsBuilder : GlobalSettingsBuilder<object>
    {
        public GlobalSettingsBuilder() : base(null)
        {
        }
    }

    public class GlobalSettingsBuilder<TParent> : ChildBuilderBase<TParent, IGlobalSettings>

    {
        private string _configurationStatus;
        private string _databaseFactoryServerVersion;
        private string _defaultUiLanguage;
        private bool? _disableElectionForSingleServer;
        private bool? _hideTopLevelNodeFromPath;
        private bool? _installEmptyDatabase;
        private bool? _installMissingDatabase;
        private bool? _isSmtpServerConfigured;
        private string _path;
        private string _registerType;
        private string _reservedPaths;
        private string _reservedUrls;
        private int? _timeOutInMinutes;
        private string _umbracoCssPath;
        private string _umbracoMediaPath;
        private string _umbracoPath;
        private string _umbracoScriptsPath;
        private bool? _useHttps;
        private int? _versionCheckPeriod;


        public GlobalSettingsBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

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

        public GlobalSettingsBuilder<TParent> WithIsSmtpServerConfigured(bool isSmtpServerConfigured)
        {
            _isSmtpServerConfigured = isSmtpServerConfigured;
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

        public GlobalSettingsBuilder<TParent> WithUmbracoPath(string umbracoPath)
        {
            _umbracoPath = umbracoPath;
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

        public override IGlobalSettings Build()
        {
            var configurationStatus = _configurationStatus ?? "9.0.0";
            var databaseFactoryServerVersion = _databaseFactoryServerVersion ?? null;
            var defaultUiLanguage = _defaultUiLanguage ?? "en";
            var disableElectionForSingleServer = _disableElectionForSingleServer ?? false;
            var hideTopLevelNodeFromPath = _hideTopLevelNodeFromPath ?? false;
            var installEmptyDatabase = _installEmptyDatabase ?? false;
            var installMissingDatabase = _installMissingDatabase ?? false;
            var isSmtpServerConfigured = _isSmtpServerConfigured ?? false;
            var path = _path ?? "/umbraco";
            var registerType = _registerType ?? null;
            var reservedPaths = _reservedPaths ?? "~/app_plugins/,~/install/,~/mini-profiler-resources/,";
            var reservedUrls = _reservedUrls ?? "~/config/splashes/noNodes.aspx,~/.well-known,";
            var umbracoPath = _umbracoPath ?? "~/umbraco";
            var useHttps = _useHttps ?? false;
            var umbracoCssPath = _umbracoCssPath ?? "~/css";
            var umbracoMediaPath = _umbracoMediaPath ?? "~/media";
            var umbracoScriptsPath = _umbracoScriptsPath ?? "~/scripts";
            var versionCheckPeriod = _versionCheckPeriod ?? 0;
            var timeOutInMinutes = _timeOutInMinutes ?? 20;


            return new TestGlobalSettings
            {
                ConfigurationStatus = configurationStatus,
                DatabaseFactoryServerVersion = databaseFactoryServerVersion,
                DefaultUILanguage = defaultUiLanguage,
                DisableElectionForSingleServer = disableElectionForSingleServer,
                HideTopLevelNodeFromPath = hideTopLevelNodeFromPath,
                InstallEmptyDatabase = installEmptyDatabase,
                InstallMissingDatabase = installMissingDatabase,
                IsSmtpServerConfigured = isSmtpServerConfigured,
                Path = path,
                RegisterType = registerType,
                ReservedPaths = reservedPaths,
                ReservedUrls = reservedUrls,
                UmbracoPath = umbracoPath,
                UseHttps = useHttps,
                UmbracoCssPath = umbracoCssPath,
                UmbracoMediaPath = umbracoMediaPath,
                UmbracoScriptsPath = umbracoScriptsPath,
                VersionCheckPeriod = versionCheckPeriod,
                TimeOutInMinutes = timeOutInMinutes
            };
        }

        private class TestGlobalSettings : IGlobalSettings
        {
            public string ReservedUrls { get; set; }
            public string ReservedPaths { get; set; }
            public string Path { get; set; }
            public string ConfigurationStatus { get; set; }
            public int TimeOutInMinutes { get; set; }
            public string DefaultUILanguage { get; set; }
            public bool HideTopLevelNodeFromPath { get; set; }
            public bool UseHttps { get; set; }
            public int VersionCheckPeriod { get; set; }
            public string UmbracoPath { get; set; }
            public string UmbracoCssPath { get; set; }
            public string UmbracoScriptsPath { get; set; }
            public string UmbracoMediaPath { get; set; }
            public bool IsSmtpServerConfigured { get; set; }
            public bool InstallMissingDatabase { get; set; }
            public bool InstallEmptyDatabase { get; set; }
            public bool DisableElectionForSingleServer { get; set; }
            public string RegisterType { get; set; }
            public string DatabaseFactoryServerVersion { get; set; }
        }
    }
}
