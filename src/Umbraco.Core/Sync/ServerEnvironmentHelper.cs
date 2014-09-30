using System;
using System.Linq;
using System.Web;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A helper used to determine the current server environment status
    /// </summary>
    internal static class ServerEnvironmentHelper
    {
        /// <summary>
        /// Returns the current umbraco base url for the current server depending on it's environment 
        /// status. This will attempt to determine the internal umbraco base url that can be used by the current
        /// server to send a request to itself if it is in a load balanced environment.
        /// </summary>
        /// <returns>The full base url including schema (i.e. http://myserver:80/umbraco )</returns>
        public static string GetCurrentServerUmbracoBaseUrl()
        {
            var status = GetStatus();

            if (status == CurrentServerEnvironmentStatus.Single)
            {
                //if it's a single install, then the base url has to be the first url registered. Use HTTP or HTTPS as appropriate.
                if (GlobalSettings.UseSSL)
                {
                    return string.Format("https://{0}", ApplicationContext.Current.OriginalRequestUrl);
                }
                else
                {
                    return string.Format("http://{0}", ApplicationContext.Current.OriginalRequestUrl);
                }
            }

            var servers = UmbracoConfig.For.UmbracoSettings().DistributedCall.Servers.ToArray();

            if (servers.Any() == false)
            {
                //cannot be determined, then the base url has to be the first url registered. Use HTTP or HTTPS as appropriate.
                if (GlobalSettings.UseSSL)
                {
                    return string.Format("https://{0}", ApplicationContext.Current.OriginalRequestUrl);
                }
                else
                {
                    return string.Format("http://{0}", ApplicationContext.Current.OriginalRequestUrl);
                }
            }

            foreach (var server in servers)
            {
                var appId = server.AppId;
                var serverName = server.ServerName;

                if (appId.IsNullOrWhiteSpace() && serverName.IsNullOrWhiteSpace())
                {
                    continue;
                }

                if ((appId.IsNullOrWhiteSpace() == false && appId.Trim().InvariantEquals(HttpRuntime.AppDomainAppId))
                    || (serverName.IsNullOrWhiteSpace() == false && serverName.Trim().InvariantEquals(NetworkHelper.MachineName)))
                {
                    //match by appId or computer name! return the url configured
                    return string.Format("{0}://{1}:{2}/{3}",
                        server.ForceProtocol.IsNullOrWhiteSpace() ? "http" : server.ForceProtocol,
                        server.ServerAddress,
                        server.ForcePortnumber.IsNullOrWhiteSpace() ? "80" : server.ForcePortnumber,
                        IOHelper.ResolveUrl(SystemDirectories.Umbraco).TrimStart('/'));
                }                
            }

            //cannot be determined, then the base url has to be the first url registered. Use HTTP or HTTPS as appropriate.
            if (GlobalSettings.UseSSL)
            {
                return string.Format("https://{0}", ApplicationContext.Current.OriginalRequestUrl);
            }
            else
            {
                return string.Format("http://{0}", ApplicationContext.Current.OriginalRequestUrl);
            }
        }

        /// <summary>
        /// Returns the current environment status for the current server
        /// </summary>
        /// <returns></returns>
        public static CurrentServerEnvironmentStatus GetStatus()
        {
            if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled == false)
            {
                return CurrentServerEnvironmentStatus.Single;
            }

            var servers = UmbracoConfig.For.UmbracoSettings().DistributedCall.Servers.ToArray();

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