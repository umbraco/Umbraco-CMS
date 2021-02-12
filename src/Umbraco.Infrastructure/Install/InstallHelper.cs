using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Install
{
    public sealed class InstallHelper
    {
        private static HttpClient _httpClient;
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly ILogger<InstallHelper> _logger;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly ConnectionStrings _connectionStrings;
        private readonly IInstallationService _installationService;
        private readonly ICookieManager _cookieManager;
        private readonly IUserAgentProvider _userAgentProvider;
        private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private InstallationType? _installationType;

        public InstallHelper(DatabaseBuilder databaseBuilder,
            ILogger<InstallHelper> logger,
            IUmbracoVersion umbracoVersion,
            IOptions<ConnectionStrings> connectionStrings,
            IInstallationService installationService,
            ICookieManager cookieManager,
            IUserAgentProvider userAgentProvider,
            IUmbracoDatabaseFactory umbracoDatabaseFactory,
            IJsonSerializer jsonSerializer)
        {
            _logger = logger;
            _umbracoVersion = umbracoVersion;
            _databaseBuilder = databaseBuilder;
            _connectionStrings = connectionStrings.Value ?? throw new ArgumentNullException(nameof(connectionStrings));
            _installationService = installationService;
            _cookieManager = cookieManager;
            _userAgentProvider = userAgentProvider;
            _umbracoDatabaseFactory = umbracoDatabaseFactory;
            _jsonSerializer = jsonSerializer;

            //We need to initialize the type already, as we can't detect later, if the connection string is added on the fly.
            GetInstallationType();
        }

        public InstallationType GetInstallationType()
        {
            return _installationType ?? (_installationType = IsBrandNewInstall ? InstallationType.NewInstall : InstallationType.Upgrade).Value;
        }

        public async Task SetInstallStatusAsync(bool isCompleted, string errorMsg)
        {
            try
            {
                var userAgent = _userAgentProvider.GetUserAgent();

                // Check for current install Id
                var installId = Guid.NewGuid();

                var installCookie = _cookieManager.GetCookieValue(Constants.Web.InstallerCookieName);
                if (string.IsNullOrEmpty(installCookie) == false)
                {
                    if (Guid.TryParse(installCookie, out installId))
                    {
                        // check that it's a valid Guid
                        if (installId == Guid.Empty)
                            installId = Guid.NewGuid();
                    }
                    else
                    {
                        installId = Guid.NewGuid(); // Guid.TryParse will have reset installId to Guid.Empty
                    }
                }

                _cookieManager.SetCookieValue(Constants.Web.InstallerCookieName, installId.ToString());

                var dbProvider = string.Empty;
                if (IsBrandNewInstall == false)
                {
                    // we don't have DatabaseProvider anymore... doing it differently
                    //dbProvider = ApplicationContext.Current.DatabaseContext.DatabaseProvider.ToString();
                    dbProvider = _umbracoDatabaseFactory.SqlContext.SqlSyntax.DbProvider;
                }

                var installLog = new InstallLog(installId: installId, isUpgrade: IsBrandNewInstall == false,
                    installCompleted: isCompleted, timestamp: DateTime.Now, versionMajor: _umbracoVersion.Current.Major,
                    versionMinor: _umbracoVersion.Current.Minor, versionPatch: _umbracoVersion.Current.Build,
                    versionComment: _umbracoVersion.Comment, error: errorMsg, userAgent: userAgent,
                    dbProvider: dbProvider);

                await _installationService.LogInstall(installLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in InstallStatus trying to check upgrades");
            }
        }

        /// <summary>
        /// Checks if this is a brand new install meaning that there is no configured version and there is no configured database connection
        /// </summary>
        private bool IsBrandNewInstall
        {
            get
            {
                var databaseSettings = _connectionStrings.UmbracoConnectionString;
                if (databaseSettings.IsConnectionStringConfigured() == false)
                {
                    //no version or conn string configured, must be a brand new install
                    return true;
                }

                //now we have to check if this is really a new install, the db might be configured and might contain data

                if (databaseSettings.IsConnectionStringConfigured() == false
                    || _databaseBuilder.IsDatabaseConfigured == false)
                {
                    return true;
                }

                return _databaseBuilder.IsUmbracoInstalled() == false;
            }
        }

        public IEnumerable<Package> GetStarterKits()
        {
            if (_httpClient == null)
                _httpClient = new HttpClient();

            var packages = new List<Package>();
            try
            {
                var requestUri = $"https://our.umbraco.com/webapi/StarterKit/Get/?umbracoVersion={_umbracoVersion.Current}";

                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    var response = _httpClient.SendAsync(request).Result;


                    var json = response.Content.ReadAsStringAsync().Result;
                    packages = _jsonSerializer.Deserialize<IEnumerable<Package>>(json).ToList();
                }
            }
            catch (AggregateException ex)
            {
                _logger.LogError(ex, "Could not download list of available starter kits");
            }

            return packages;
        }
    }
}
