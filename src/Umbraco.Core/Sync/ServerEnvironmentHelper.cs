using System.Linq;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A helper used to determine the current server environment status
    /// </summary>
    internal static class ServerEnvironmentHelper
    {
        public static void TrySetApplicationUrlFromSettings(ApplicationContext appContext, ILogger logger, IUmbracoSettingsSection settings)
        {
            // try umbracoSettings:settings/web.routing/@umbracoApplicationUrl
            // which is assumed to:
            // - end with SystemDirectories.Umbraco
            // - contain a scheme
            // - end or not with a slash, it will be taken care of
            // eg "http://www.mysite.com/umbraco"
            var url = settings.WebRouting.UmbracoApplicationUrl;
            if (url.IsNullOrWhiteSpace() == false)
            {
                appContext.UmbracoApplicationUrl = url.TrimEnd('/');
                logger.Info<ApplicationContext>("ApplicationUrl: " + appContext.UmbracoApplicationUrl + " (using web.routing/@umbracoApplicationUrl)");
                return;
            }

            // try umbracoSettings:settings/scheduledTasks/@baseUrl
            // which is assumed to:
            // - end with SystemDirectories.Umbraco
            // - NOT contain any scheme (because, legacy)
            // - end or not with a slash, it will be taken care of
            // eg "mysite.com/umbraco"
            url = settings.ScheduledTasks.BaseUrl;
            if (url.IsNullOrWhiteSpace() == false)
            {
                var ssl = GlobalSettings.UseSSL ? "s" : "";
                url = "http" + ssl + "://" + url;
                appContext.UmbracoApplicationUrl = url.TrimEnd('/');
                logger.Info<ApplicationContext>("ApplicationUrl: " + appContext.UmbracoApplicationUrl + " (using scheduledTasks/@baseUrl)");
                return;
            }

            // try servers
            var status = GetStatus(settings);
            if (status == CurrentServerEnvironmentStatus.Single)
                return;

            // no server, nothing we can do
            var servers = settings.DistributedCall.Servers.ToArray();
            if (servers.Length == 0)
                return;

            // we have servers, look for this server
            foreach (var server in servers)
            {
                var appId = server.AppId;
                var serverName = server.ServerName;

                // skip if no data
                if (appId.IsNullOrWhiteSpace() && serverName.IsNullOrWhiteSpace())
                    continue;

                // if this server, build and return the url
                if ((appId.IsNullOrWhiteSpace() == false && appId.Trim().InvariantEquals(HttpRuntime.AppDomainAppId))
                    || (serverName.IsNullOrWhiteSpace() == false && serverName.Trim().InvariantEquals(NetworkHelper.MachineName)))
                {
                    // match by appId or computer name, return the url configured
                    url = string.Format("{0}://{1}:{2}/{3}",
                        server.ForceProtocol.IsNullOrWhiteSpace() ? "http" : server.ForceProtocol,
                        server.ServerAddress,
                        server.ForcePortnumber.IsNullOrWhiteSpace() ? "80" : server.ForcePortnumber,
                        IOHelper.ResolveUrl(SystemDirectories.Umbraco).TrimStart('/'));

                    appContext.UmbracoApplicationUrl = url.TrimEnd('/');
                    logger.Info<ApplicationContext>("ApplicationUrl: " + appContext.UmbracoApplicationUrl + " (using distributedCall/servers)");
                }
            }
        }

        /// <summary>
        /// Returns the current environment status for the current server
        /// </summary>
        /// <returns></returns>
        public static CurrentServerEnvironmentStatus GetStatus(IUmbracoSettingsSection settings)
        {
            if (settings.DistributedCall.Enabled == false)
            {
                return CurrentServerEnvironmentStatus.Single;
            }

            var servers = settings.DistributedCall.Servers.ToArray();

            if (servers.Any() == false)
            {
                return CurrentServerEnvironmentStatus.Unknown;
            }

            var master = servers.FirstOrDefault();

            if (master == null)
            {
                return CurrentServerEnvironmentStatus.Unknown;
            }

            //we determine master/slave based on the first server registered
            //TODO: In v7 we have publicized ServerRegisterResolver - we won't be able to determine this based on that
            // but we'd need to change the IServerAddress interfaces which is breaking.

            var appId = master.AppId;
            var serverName = master.ServerName;

            if (appId.IsNullOrWhiteSpace() && serverName.IsNullOrWhiteSpace())
            {
                return CurrentServerEnvironmentStatus.Unknown;
            }

            if ((appId.IsNullOrWhiteSpace() == false && appId.Trim().InvariantEquals(HttpRuntime.AppDomainAppId))
                    || (serverName.IsNullOrWhiteSpace() == false && serverName.Trim().InvariantEquals(NetworkHelper.MachineName)))
            {
                //match by appdid or server name!
                return CurrentServerEnvironmentStatus.Master;
            }

            return CurrentServerEnvironmentStatus.Slave;
        }
    }
}