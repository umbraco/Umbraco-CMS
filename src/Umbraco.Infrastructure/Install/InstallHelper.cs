using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Install
{
    public sealed class InstallHelper
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly ILogger<InstallHelper> _logger;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
        private readonly IInstallationService _installationService;
        private readonly ICookieManager _cookieManager;
        private readonly IUserAgentProvider _userAgentProvider;
        private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
        private readonly IFireAndForgetRunner _fireAndForgetRunner;
        private readonly IEnumerable<IDatabaseProviderMetadata> _databaseProviderMetadata;

        public InstallHelper(
            DatabaseBuilder databaseBuilder,
            ILogger<InstallHelper> logger,
            IUmbracoVersion umbracoVersion,
            IOptionsMonitor<ConnectionStrings> connectionStrings,
            IInstallationService installationService,
            ICookieManager cookieManager,
            IUserAgentProvider userAgentProvider,
            IUmbracoDatabaseFactory umbracoDatabaseFactory,
            IFireAndForgetRunner fireAndForgetRunner,
            IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata)
        {
            _logger = logger;
            _umbracoVersion = umbracoVersion;
            _databaseBuilder = databaseBuilder;
            _connectionStrings = connectionStrings;
            _installationService = installationService;
            _cookieManager = cookieManager;
            _userAgentProvider = userAgentProvider;
            _umbracoDatabaseFactory = umbracoDatabaseFactory;
            _fireAndForgetRunner = fireAndForgetRunner;
            _databaseProviderMetadata = databaseProviderMetadata;
        }

        public Task SetInstallStatusAsync(bool isCompleted, string errorMsg)
        {
            try
            {
                var userAgent = _userAgentProvider.GetUserAgent();

                // Check for current install ID
                var installCookie = _cookieManager.GetCookieValue(Constants.Web.InstallerCookieName);
                if (!Guid.TryParse(installCookie, out Guid installId))
                {
                    installId = Guid.NewGuid();

                    _cookieManager.SetCookieValue(Constants.Web.InstallerCookieName, installId.ToString(), false);
                }

                var dbProvider = string.Empty;
                if (IsBrandNewInstall == false)
                {
                    // we don't have DatabaseProvider anymore... doing it differently
                    //dbProvider = ApplicationContext.Current.DatabaseContext.DatabaseProvider.ToString();
                    dbProvider = _umbracoDatabaseFactory.SqlContext.SqlSyntax.DbProvider;
                }

                var installLog = new InstallLog(
                    installId: installId,
                    isUpgrade: IsBrandNewInstall == false,
                    installCompleted: isCompleted,
                    timestamp: DateTime.Now,
                    versionMajor: _umbracoVersion.Version.Major,
                    versionMinor: _umbracoVersion.Version.Minor,
                    versionPatch: _umbracoVersion.Version.Build,
                    versionComment: _umbracoVersion.Comment,
                    error: errorMsg,
                    userAgent: userAgent,
                    dbProvider: dbProvider);

                _fireAndForgetRunner.RunFireAndForget(() => _installationService.LogInstall(installLog));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in InstallStatus trying to check upgrades");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks if this is a brand new install, meaning that there is no configured database connection or the database is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this is a brand new install; otherwise, <c>false</c>.
        /// </value>
        private bool IsBrandNewInstall =>
            _connectionStrings.CurrentValue.IsConnectionStringConfigured() == false ||
            _databaseBuilder.IsDatabaseConfigured == false ||
            (_databaseBuilder.CanConnectToDatabase == false && _databaseProviderMetadata.CanForceCreateDatabase(_umbracoDatabaseFactory)) ||
            _databaseBuilder.IsUmbracoInstalled() == false;
    }
}
