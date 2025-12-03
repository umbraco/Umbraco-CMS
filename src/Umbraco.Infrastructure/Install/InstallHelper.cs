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

namespace Umbraco.Cms.Infrastructure.Install
{
    [Obsolete("InstallHelper is no longer used internally and will be removed in Umbraco 19.")]
    public sealed class InstallHelper
    {

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
        }

        /// <summary>
        ///     This method used to send installer telemetry to Our.Umbraco.com but no longer does anything and will be removed in Umbraco 19.
        /// </summary>
        [Obsolete("SetInstallStatusAsync no longer has any function and will be removed in Umbraco 19.")]
        public Task SetInstallStatusAsync(bool isCompleted, string errorMsg) => Task.CompletedTask;
    }
}
