using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;

namespace Umbraco.Web.HealthCheck.Checks.Config
{

    [HealthCheck(
          "65dc4b91-e2f4-4aaf-92b8-3d6fa37064ff",
          "Load Balancing Configuration",
          Description = "Checks the status of the site for Load Balancing Configuration",
          Group = "Configuration")]
    public class LoadBalancingHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        public LoadBalancingHealthCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }
        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            // we only want to run some of these checks if it is appropriate to do so, eg not worth checking distributed calls server settings if distributed calls is turned off;
            var statusesToCheck = new List<HealthCheckStatus>();

            statusesToCheck.Add(DisplayUmbracoApplicationUrl());
            statusesToCheck.Add(DisplayHowUmbracoApplicationUrlSet());

            // only run these checks if the umbracoApplicationUrl has been manually set
            if (!UmbracoConfig.For.UmbracoSettings().WebRouting.UmbracoApplicationUrl.IsNullOrWhiteSpace())
            {
                statusesToCheck.Add(CheckUmbracoApplicationUrlEndsWithUmbraco());
                statusesToCheck.Add(CheckUmbracoApplicationUrlUsesHTTPSOrNot());
            }

            statusesToCheck.Add(CheckMakeRequestToUmbracoApplicationUrl());

            //Traditional Load Balancing
            statusesToCheck.Add(CheckDistributedCallsSetting());
            // only run these checks if distributed calls are enabled
            if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled)
            {
                statusesToCheck.Add(CheckDistributedCallsServers());
            }
            //Single Server or Flexible Load Balancing
            else
            {
                statusesToCheck.Add(CheckElectionDisabledForSingleServer());
                statusesToCheck.Add(DisplayCurrentServerRole());
                statusesToCheck.Add(DisplayCurrentServerIdentity());
                statusesToCheck.Add(DisplayFlexibleLoadBalancingServers());
            }
            return statusesToCheck;
        }

        ///Just display the UmbracoApplicationUrl from the ApplicationContext, eg what Umbraco has chosen/or has been configured.
        private HealthCheckStatus DisplayUmbracoApplicationUrl()
        {
            string resultMessage;
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();
            var umbracoApplicationUrl = ApplicationContext.Current.UmbracoApplicationUrl;
            resultMessage = "Umbraco Application Url: " + umbracoApplicationUrl;
            // resultMessage = _textService.Localize("healthcheck/umbracoDisplayUmbracoApplicationUrl");
            return
            new HealthCheckStatus(resultMessage)
            {
                ResultType = resultType,
                Actions = actions
            };
        }
        /// <summary>
        /// quick check to see if the umbracoApplicationUrl has been set in the config or left blank
        /// </summary>
        /// <returns></returns>
        private HealthCheckStatus DisplayHowUmbracoApplicationUrlSet()
        {
            string resultMessage;
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();
            var configuredUmbracoApplicationUrl = UmbracoConfig.For.UmbracoSettings().WebRouting.UmbracoApplicationUrl;
            if (!configuredUmbracoApplicationUrl.IsNullOrWhiteSpace())
            {
                resultMessage = "umbracoApplicationUrl has been configured in umbracoSettings.config routing element";
            }
            else
            {
                // it may have been set by a custom registraar or Umbraco has guessed based on the first request
                // not sure yet how to tell the difference here
                resultMessage = "umbracoApplicationUrl has been set during Application Start up based on the Url of the first request made to Umbraco or by a custom Registrar implementation";
            }
            return
            new HealthCheckStatus(resultMessage)
            {
                ResultType = resultType,
                Actions = actions
            };
        }
        /// <summary>
        /// check if the UmbracoApplicationUrl has been configured, whether it has been done so correctly eg does it end with /umbraco - a common mistake
        /// </summary>
        /// <returns></returns>
        private HealthCheckStatus CheckUmbracoApplicationUrlEndsWithUmbraco()
        {
            string resultMessage = string.Empty;
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();
            var configuredUmbracoApplicationUrl = UmbracoConfig.For.UmbracoSettings().WebRouting.UmbracoApplicationUrl;
            if (!configuredUmbracoApplicationUrl.IsNullOrWhiteSpace())
            {
                var umbracoApplicationUri = new Uri(configuredUmbracoApplicationUrl);
                //check it ends in /umbraco
                var lastSegment = umbracoApplicationUri.Segments[umbracoApplicationUri.Segments.Length - 1];
                if (!"umbraco".InvariantEquals(lastSegment))
                {
                    resultMessage = "umbracoApplicationUrl needs to end in /umbraco - check the configuration in umbracoSettings.config routing element";
                    //ToDo resultMessage = _textService.Localize("healthcheck/umbracoApplicationUrlEndingInUmbraco");
                    resultType = StatusResultType.Warning;
                }
            }
            else
            {

                resultMessage = "Umbraco Application Url ends in /Umbraco";
                resultType = StatusResultType.Success;
            }
            return
                       new HealthCheckStatus(resultMessage)
                       {
                           ResultType = resultType,
                           Actions = actions
                       };


        }
        /// <summary>
        /// a common mistake is not to set Https at the beginning of the Url, if the site runs over SSL, or HTTPS is set but UmbracoUseSSL app setting is false
        /// </summary>
        /// <returns></returns>
        private HealthCheckStatus CheckUmbracoApplicationUrlUsesHTTPSOrNot()
        {
            string resultMessage = string.Empty;
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();
            //check if useSSL is configured that this url starts with https
            var useSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["umbracoUseSSL"]);
            var configuredUmbracoApplicationUrl = UmbracoConfig.For.UmbracoSettings().WebRouting.UmbracoApplicationUrl;
            if (!configuredUmbracoApplicationUrl.IsNullOrWhiteSpace())
            {
                if (useSSL && !configuredUmbracoApplicationUrl.StartsWith("https"))
                {
                    resultMessage = resultMessage + "The AppSetting UmbracoUseSSL is set to true, however your configured UmbracoApplicationUrl does not start with https";
                    // resultMessage = _textService.Localize("healthcheck/umbracoApplicationUrlEndingInUmbraco");
                    resultType = StatusResultType.Warning;
                }
                // check if umbracoApplicationUrl starts with https that useSSL is also true
                if (!useSSL && configuredUmbracoApplicationUrl.StartsWith("https"))
                {
                    resultMessage = resultMessage + "The AppSetting UmbracoUseSSL is set to false, however your configured UmbracoApplicationUrl starts with https - to work over https, UmbracoUseSSL must be true.";
                    // resultMessage = _textService.Localize("healthcheck/umbracoApplicationUrlEndingInUmbraco");
                    resultType = StatusResultType.Warning;
                }
            }
            else
            {
                resultMessage = useSSL ? "Umbraco Application Url is configured to run over SSL" : "Umbraco Application Url is configured to not run over SSL";
                resultType = StatusResultType.Info;
            }
            return
                   new HealthCheckStatus(resultMessage)
                   {
                       ResultType = resultType,
                       Actions = actions
                   };



        }
        /// <summary>
        /// Make a web request to UmbracoApplicationUrl - the server needs to be able to resolve this address for Load Balancing to work
        /// </summary>
        /// <returns></returns>
        private HealthCheckStatus CheckMakeRequestToUmbracoApplicationUrl()
        {
            string resultMessage;
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();
            var umbracoApplicationUrl = ApplicationContext.Current.UmbracoApplicationUrl;
            try
            {
                HttpWebRequest requestToUmbracoApplicationUrl = (HttpWebRequest)WebRequest.Create(umbracoApplicationUrl + "/ping.aspx");
                HttpWebResponse responseFromUmbracoApplicationUrl = (HttpWebResponse)requestToUmbracoApplicationUrl.GetResponse();
                if (responseFromUmbracoApplicationUrl.StatusCode == HttpStatusCode.OK)
                {
                    resultMessage = "Server can make a successful web request to the Umbraco Application Url";
                    resultType = StatusResultType.Success;
                }
                else
                {
                    resultMessage = "server tried to make a web request to the Umbraco Application Url but failed with status code: " + responseFromUmbracoApplicationUrl.StatusCode.ToString();
                    resultType = StatusResultType.Warning;
                }
                responseFromUmbracoApplicationUrl.Close();
            }
            catch (WebException ex)
            {
                resultMessage = "A Web exception occurred when this server tried to make a web request to the Umbraco Application Url: " + ex.Message;
                resultType = StatusResultType.Warning;
            }
            catch (Exception ex)
            {
                resultMessage = "A General exception occurred when this server tried to make a web request to the Umbraco Application Url: " + ex.Message;
                resultType = StatusResultType.Warning;
            }
            return
              new HealthCheckStatus(resultMessage)
              {
                  ResultType = resultType,
                  Actions = actions
              };
        }
        /// <summary>
        /// Check whether Distributed Calls is turned on
        /// </summary>
        /// <returns></returns>
        private HealthCheckStatus CheckDistributedCallsSetting()
        {
            string resultMessage;
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();

            if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled)
            {
                // flavourOfLoadingBalancing = "Traditional Load Balancing";
                resultMessage = "Traditional Load Balancing is enabled (Distributed Calls are turned on in UmbracoSettings.Config) - This turns off Flexible Load Balancing";

            }
            else
            {
                resultMessage = "Traditional Load Balancing is disabled (Distributed Calls are turned off in UmbracoSettings.Config)";
            }
            return
       new HealthCheckStatus(resultMessage)
       {
           ResultType = resultType,
           Actions = actions
       };
        }
        /// <summary>
        ///  check the status of configured servers for traditional load balancing
        /// </summary>
        /// <returns></returns>
        private HealthCheckStatus CheckDistributedCallsServers()
        {
            string resultMessage = String.Empty;
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();
            //check if any servers are listed
            if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Servers.Any())
            {
                if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Servers.Count() == 1)
                {
                    // in the circumstance where only one server is listed, then this server should be the server we are currently on (this is a dark arts method for turning off flexible load balancing and works by luck)
                    var server = UmbracoConfig.For.UmbracoSettings().DistributedCall.Servers.FirstOrDefault();
                    if (server.ServerName == Environment.MachineName)
                    {
                        resultMessage = "Only one server configured for Traditional Load Balancing - This effectively turns off flexible load balancing, and works by luck; consider using the AppSetting umbracoDisableElectionForSingleServer setting to true for the same result";
                    }
                    else
                    {
                        resultMessage = "Only one server configured for Traditional Load Balancing. Add details of the servers you intend to load balance in UmbracoSettings.config DistrubutedCalls Servers section or disable Distributed Calls to use Flexible Load Balancing or set the AppSetting umbracoDisableElectionForSingleServer to true to disable Load Balancing";
                    }
                }
                else
                {
                    foreach (var server in UmbracoConfig.For.UmbracoSettings().DistributedCall.Servers)
                    {
                        if (string.IsNullOrWhiteSpace(server.ServerAddress))
                        {
                            resultMessage = resultMessage + "An empty server element exists in UmbracoSettings DistributedCalls Server List";
                            resultType = StatusResultType.Warning;
                        }
                        else
                        {

                            if (String.IsNullOrEmpty(server.ServerName) && String.IsNullOrEmpty(server.AppId))
                            {
                                resultMessage = resultMessage + server.ServerAddress + " - Missing ServerName or IIS AppId in UmbracoSettings distributed calls server list | ";
                                resultType = StatusResultType.Warning;
                            }
                            else
                            {
                                resultMessage = resultMessage + server.ServerAddress + " | ";
                            }
                            // TODO: would it be excessive here to see if we can ping the cache refresher url for each of these servers?
                            // bearing in mind it would trigger the cache to be refreshed :-)
                            // or should we just see if ping.aspx works?
                        }
                    }
                }
            }
            else
            {
                // no servers listed this
                resultMessage = resultMessage + "No servers configured for Traditional Load Balancing. Add details of the servers you intend to load balance in UmbracoSettings.config DistributedCalls Servers section or disable Distributed Calls to use Flexible Load Balancing or set the AppSetting umbracoDisableElectionForSingleServer to true to disable Load Balancing";
                resultType = StatusResultType.Warning;
            }


            return
 new HealthCheckStatus(resultMessage)
 {
     ResultType = resultType,
     Actions = actions
 };

        }
        /// <summary>
        /// Use Registrar to determine the current server role
        /// </summary>
        /// <returns></returns>
        private HealthCheckStatus DisplayCurrentServerRole()
        {
            string resultMessage = String.Empty;
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();

            //make sure that distributed calls is not turned on otherwise not safe to try and read from registrar? and cast to IServerRegistrar2

            // check umbracoDisableElectionForSingleServer not turned on
            //if (Convert.ToBoolean(ConfigurationManager.AppSettings["umbracoDisableElectionForSingleServer"]))
            //{
            //    resultMessage = "ServerRole:Single - No Load Balancing";
            //}

            //Server Election Role Status, what is the role that the server thinks it is
            var currentServerRegistrar = (IServerRegistrar2)ServerRegistrarResolver.Current.Registrar;
            var currentServerRole = currentServerRegistrar.GetCurrentServerRole();
            switch (currentServerRole)
            {
                case ServerRole.Slave:
                    resultMessage = "ServerRole:Slave - Flexible Load Balancing";
                    break;
                case ServerRole.Master:
                    resultMessage = "ServerRole:Master - Flexible Load Balancing";
                    break;
                case ServerRole.Single:
                    resultMessage = "ServerRole:Single - No Load Balancing";
                    break;
                case ServerRole.Unknown:
                    resultMessage = "ServerRole:Unknown - Umbraco could not determine it's ServerRole, there must be a configuration issue";
                    resultType = StatusResultType.Warning;
                    break;
            }
            resultMessage = resultMessage + " | This was determined by the Registrar: " + currentServerRegistrar.GetType().Name;

            return
        new HealthCheckStatus(resultMessage)
        {
            ResultType = resultType,
            Actions = actions
        };
        }

        /// <summary>
        /// Check whether appsetting has disabled election, eg on Cloud
        /// </summary>
        /// <returns></returns>
        private HealthCheckStatus CheckElectionDisabledForSingleServer()
        {
            string resultMessage = String.Empty;
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();

            //if appsetting umbracoDisableElectionForSingleServer is set - election won't occur (used on cloud) - server is a single server and no election / flexible load balancing will take place
            var isElectionForSingleServerDisabled = Convert.ToBoolean(ConfigurationManager.AppSettings["umbracoDisableElectionForSingleServer"]);
            if (isElectionForSingleServerDisabled)
            {
                resultMessage = resultMessage + "The appsetting umbracoDisableElectionForSingleServer has been set to true - Turning off Flexible Load Balancing - correct setting for Umbraco Cloud";
                resultType = StatusResultType.Info;
            }
            else
            {
                // show this message when it's not there or turned off?
                resultMessage = resultMessage + "The appsetting umbracoDisableElectionForSingleServer is false - (This is correct for flexible load balancing)";
                resultType = StatusResultType.Info;
            }
            return
            new HealthCheckStatus(resultMessage)
            {
                ResultType = resultType,
                Actions = actions
            };
        }
        private HealthCheckStatus DisplayCurrentServerIdentity()
        {
            string resultMessage = "";
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();
            //finally for Slave and Master configurations we can look up in the umbracoServer table to see what other servers are in play...

            var serverRegistrationService = ApplicationContext.Current.Services.ServerRegistrationService;
            // wonder if it's useful to see what the ServerRegistrationService returns for role
            var currentServerRole = serverRegistrationService.GetCurrentServerRole();
            var currentServerIdentity = serverRegistrationService.CurrentServerIdentity;
            resultMessage = "Identity: " + currentServerIdentity + " | Role: " + currentServerRole;

            return
               new HealthCheckStatus(resultMessage)
               {
                   ResultType = resultType,
                   Actions = actions
               };
        }
        /// <summary>
        /// List out active servers from umbracoServer database table
        /// </summary>
        /// <returns></returns>
        private HealthCheckStatus DisplayFlexibleLoadBalancingServers()
        {
            string resultMessage = "";
            StatusResultType resultType = StatusResultType.Info;
            // can we fix anything
            var actions = new List<HealthCheckAction>();
            //finally for Slave and Master configurations we can look up in the umbracoServer table to see what other servers are in play...

            var serverRegistrationService = ApplicationContext.Current.Services.ServerRegistrationService;
            var currentServerRole = serverRegistrationService.GetCurrentServerRole();
            var activeServers = serverRegistrationService.GetActiveServers();

            if (activeServers.Any())
            {
                //how many servers
                if (activeServers.Count() == 1)
                {
                    resultMessage = "There is only one active Server in the Flexible Load Balancing Pool";
                }
                else
                {
                    resultMessage = "There are " + activeServers.Count() + " active servers in the Flexible Load Balancing Pool";
                }
                //is it appropriate to use html here?
                resultMessage = resultMessage + "<ul>";
                foreach (var server in activeServers)
                {

                    resultMessage = resultMessage + "<li>";
                    resultMessage = resultMessage + server.ServerAddress + " | " + server.ServerIdentity + " | Registered: " + server.Registered + " | Master: " + server.IsMaster + " | Last Accessed: " + server.Accessed;
                    resultMessage = resultMessage + "</li>";
                }
                resultMessage = resultMessage + "</ul>";

            }
            else
            {
                resultMessage = "0 Active servers in the Flexible Load Balancing Pool";
                if (currentServerRole == ServerRole.Master || currentServerRole == ServerRole.Slave)
                {
                    resultType = StatusResultType.Warning;
                }
                else
                {
                    resultType = StatusResultType.Info;
                }
            }

            return
                     new HealthCheckStatus(resultMessage)
                     {
                         ResultType = resultType,
                         Actions = actions
                     };
        }
        /// <summary>
        /// NO Actions...
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            // NO ACTIONS
            throw new InvalidOperationException("HttpsCheck action requested is either not executable or does not exist");

        }


    }
}
