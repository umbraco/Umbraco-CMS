using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public class UmbracoSettingsSection : ConfigurationSection
    {

        ///// <summary>
        ///// Get the current settings
        ///// </summary>
        //public static UmbracoSettings Current
        //{
        //    get { return (UmbracoSettings) ConfigurationManager.GetSection("umbracoConfiguration/settings"); }
            
        //}

        [ConfigurationProperty("content")]
        internal ContentElement Content
        {
            get { return (ContentElement)this["content"]; }
        }

        [ConfigurationProperty("security")]
        internal SecurityElement Security
        {
            get { return (SecurityElement)this["security"]; }
        }

        [ConfigurationProperty("requestHandler")]
        internal RequestHandlerElement RequestHandler
        {
            get { return (RequestHandlerElement)this["requestHandler"]; }
        }

        [ConfigurationProperty("templates")]
        internal TemplatesElement Templates
        {
            get { return (TemplatesElement)this["templates"]; }
        }

        [ConfigurationProperty("developer")]
        internal DeveloperElement Developer
        {
            get { return (DeveloperElement)this["developer"]; }
        }

        [ConfigurationProperty("viewstateMoverModule")]
        internal ViewstateMoverModuleElement ViewstateMoverModule
        {
            get { return (ViewstateMoverModuleElement)this["viewstateMoverModule"]; }
        }

        [ConfigurationProperty("logging")]
        internal LoggingElement Logging
        {
            get { return (LoggingElement)this["logging"]; }
        }

        [ConfigurationProperty("scheduledTasks")]
        internal ScheduledTasksElement ScheduledTasks
        {
            get { return (ScheduledTasksElement)this["scheduledTasks"]; }
        }

        [ConfigurationProperty("distributedCall")]
        internal DistributedCallElement DistributedCall
        {
            get { return (DistributedCallElement)this["distributedCall"]; }
        }

        [ConfigurationProperty("webservices")]
        internal WebServicesElement WebServices
        {
            get { return (WebServicesElement)this["webservices"]; }
        }

        [ConfigurationProperty("repositories")]
        internal RepositoriesElement PackageRepositories
        {
            get { return (RepositoriesElement)this["repositories"]; }
        }

        [ConfigurationProperty("providers")]
        internal ProvidersElement Providers
        {
            get { return (ProvidersElement)this["providers"]; }
        }

        [ConfigurationProperty("help")]
        internal HelpElement Help
        {
            get { return (HelpElement)this["help"]; }
        }

        [ConfigurationProperty("web.routing")]
        internal WebRoutingElement WebRouting
        {
            get { return (WebRoutingElement)this["web.routing"]; }
        }

        [ConfigurationProperty("scripting")]
        internal ScriptingElement Scripting
        {
            get { return (ScriptingElement)this["scripting"]; }
        }

    }
}
