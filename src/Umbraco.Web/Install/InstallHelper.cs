using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    public sealed class InstallHelper
    {
        private static HttpClient _httpClient;
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly HttpContextBase _httpContext;
        private readonly ILogger _logger;
        private readonly IGlobalSettings _globalSettings;
        private readonly IInstallationService _installationService;
        private InstallationType? _installationType;


        [Obsolete("Use the constructor with IInstallationService injected.")]
        public InstallHelper(
        IUmbracoContextAccessor umbracoContextAccessor,
            DatabaseBuilder databaseBuilder,
            ILogger logger,
            IGlobalSettings globalSettings)
            : this(umbracoContextAccessor, databaseBuilder, logger, globalSettings, Current.Factory.GetInstance<IInstallationService>())
        {

        }

        public InstallHelper(IUmbracoContextAccessor umbracoContextAccessor,
            DatabaseBuilder databaseBuilder,
            ILogger logger, IGlobalSettings globalSettings, IInstallationService installationService)
        {
            _httpContext = umbracoContextAccessor.UmbracoContext.HttpContext;
            _logger = logger;
            _globalSettings = globalSettings;
            _databaseBuilder = databaseBuilder;
            _installationService = installationService;
        }

        public InstallationType GetInstallationType()
        {
            return _installationType ?? (_installationType = IsBrandNewInstall ? InstallationType.NewInstall : InstallationType.Upgrade).Value;
        }

        internal async Task InstallStatus(bool isCompleted, string errorMsg)
        {
            try
            {
                var userAgent = _httpContext.Request.UserAgent;

                // Check for current install Id
                var installId = Guid.NewGuid();

                var installCookie = _httpContext.Request.GetCookieValue(Constants.Web.InstallerCookieName);
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
                _httpContext.Response.Cookies.Set(new HttpCookie(Constants.Web.InstallerCookieName, installId.ToString()));

                var dbProvider = string.Empty;
                if (IsBrandNewInstall == false)
                {
                    // we don't have DatabaseProvider anymore... doing it differently
                    //dbProvider = ApplicationContext.Current.DatabaseContext.DatabaseProvider.ToString();
                    dbProvider = GetDbProviderString(Current.SqlContext);
                }

                var installLog = new InstallLog(installId: installId, isUpgrade: IsBrandNewInstall == false,
                    installCompleted: isCompleted, timestamp: DateTime.Now, versionMajor: UmbracoVersion.Current.Major,
                    versionMinor: UmbracoVersion.Current.Minor, versionPatch: UmbracoVersion.Current.Build,
                    versionComment: UmbracoVersion.Comment, error: errorMsg, userAgent: userAgent,
                    dbProvider: dbProvider);

                await _installationService.LogInstall(installLog);
            }
            catch (Exception ex)
            {
                _logger.Error<InstallHelper>(ex, "An error occurred in InstallStatus trying to check upgrades");
            }
        }

        internal static string GetDbProviderString(ISqlContext sqlContext)
        {
            var dbProvider = string.Empty;

            // we don't have DatabaseProvider anymore...
            //dbProvider = ApplicationContext.Current.DatabaseContext.DatabaseProvider.ToString();
            //
            // doing it differently
            var syntax = sqlContext.SqlSyntax;
            if (syntax is SqlCeSyntaxProvider)
                dbProvider = "SqlServerCE";
            else if (syntax is SqlServerSyntaxProvider)
                dbProvider = (syntax as SqlServerSyntaxProvider).ServerVersion.IsAzure ? "SqlAzure" : "SqlServer";

            return dbProvider;
        }

        /// <summary>
        /// Checks if this is a brand new install meaning that there is no configured version and there is no configured database connection
        /// </summary>
        private bool IsBrandNewInstall
        {
            get
            {
                var databaseSettings = ConfigurationManager.ConnectionStrings[Constants.System.UmbracoConnectionName];
                if (_globalSettings.ConfigurationStatus.IsNullOrWhiteSpace()
                    && _databaseBuilder.IsConnectionStringConfigured(databaseSettings) == false)
                {
                    //no version or conn string configured, must be a brand new install
                    return true;
                }

                //now we have to check if this is really a new install, the db might be configured and might contain data

                if (_databaseBuilder.IsConnectionStringConfigured(databaseSettings) == false
                    || _databaseBuilder.IsDatabaseConfigured == false)
                {
                    return true;
                }

                return _databaseBuilder.IsUmbracoInstalled() == false;
            }
        }

        internal IEnumerable<Package> GetStarterKits()
        {
            if (_httpClient == null)
                _httpClient = new HttpClient();

            var packages = new List<Package>();
            try
            {
                var requestUri = $"https://our.umbraco.com/webapi/StarterKit/Get/?umbracoVersion={UmbracoVersion.Current}";

                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    var response = _httpClient.SendAsync(request).Result;
                    packages = response.Content.ReadAsAsync<IEnumerable<Package>>().Result.ToList();
                }
            }
            catch (AggregateException ex)
            {
                _logger.Error<InstallHelper>(ex, "Could not download list of available starter kits");
            }

            return packages;
        }
    }
}
