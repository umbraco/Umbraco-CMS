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
    /// <summary>
    /// Provides utility methods to assist with the installation of Umbraco CMS.
    /// </summary>
    [Obsolete("InstallHelper is no longer used internally. Scheduled for removal in Umbraco 19.")]
    public sealed class InstallHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Install.InstallHelper"/> class.
        /// </summary>
        /// <param name="databaseBuilder">The <see cref="DatabaseBuilder"/> used to manage database creation and upgrades.</param>
        /// <param name="logger">The <see cref="ILogger{InstallHelper}"/> instance for logging installation events.</param>
        /// <param name="umbracoVersion">The <see cref="IUmbracoVersion"/> providing Umbraco version information.</param>
        /// <param name="connectionStrings">The <see cref="IOptionsMonitor{ConnectionStrings}"/> for monitoring connection string changes.</param>
        /// <param name="installationService">The <see cref="IInstallationService"/> responsible for installation logic.</param>
        /// <param name="cookieManager">The <see cref="ICookieManager"/> for managing cookies during installation.</param>
        /// <param name="userAgentProvider">The <see cref="IUserAgentProvider"/> for accessing user agent information.</param>
        /// <param name="umbracoDatabaseFactory">The <see cref="IUmbracoDatabaseFactory"/> for creating Umbraco database instances.</param>
        /// <param name="fireAndForgetRunner">The <see cref="IFireAndForgetRunner"/> for running background tasks.</param>
        /// <param name="databaseProviderMetadata">A collection of <see cref="IDatabaseProviderMetadata"/> describing available database providers.</param>
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
        ///     Previously sent installer telemetry to Our.Umbraco.com, but this method is now obsolete and performs no action. Scheduled for removal in Umbraco 19.
        /// </summary>
        /// <param name="isCompleted">Indicates whether the installation is completed.</param>
        /// <param name="errorMsg">The error message if the installation failed.</param>
        /// <returns>A completed task.</returns>
        [Obsolete("SetInstallStatusAsync no longer has any function. Scheduled for removal in Umbraco 19.")]
        public Task SetInstallStatusAsync(bool isCompleted, string errorMsg) => Task.CompletedTask;
    }
}
