using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{

    public class UmbracoSettingsSection : ConfigurationSection, IUmbracoSettingsSection
    {
        [ConfigurationProperty("backOffice")]
        internal BackOfficeElement BackOffice
        {
            get { return (BackOfficeElement)this["backOffice"]; }
        }

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
        
        IContentSection IUmbracoSettingsSection.Content
        {
            get { return Content; }
        }

        ISecuritySection IUmbracoSettingsSection.Security
        {
            get { return Security; }
        }

        IRequestHandlerSection IUmbracoSettingsSection.RequestHandler
        {
            get { return RequestHandler; }
        }

        ITemplatesSection IUmbracoSettingsSection.Templates
        {
            get { return Templates; }
        }

        IBackOfficeSection IUmbracoSettingsSection.BackOffice
        {
            get { return BackOffice; }
        }

        IDeveloperSection IUmbracoSettingsSection.Developer
        {
            get { return Developer; }
        }

        IViewStateMoverModuleSection IUmbracoSettingsSection.ViewStateMoverModule
        {
            get { return ViewstateMoverModule; }
        }

        ILoggingSection IUmbracoSettingsSection.Logging
        {
            get { return Logging; }
        }

        IScheduledTasksSection IUmbracoSettingsSection.ScheduledTasks
        {
            get { return ScheduledTasks; }
        }

        IDistributedCallSection IUmbracoSettingsSection.DistributedCall
        {
            get { return DistributedCall; }
        }

        IProvidersSection IUmbracoSettingsSection.Providers
        {
            get { return Providers; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This is no longer used and will be removed in future versions")]
        IHelpSection IUmbracoSettingsSection.Help
        {
            get { return Help; }
        }

        IWebRoutingSection IUmbracoSettingsSection.WebRouting
        {
            get { return WebRouting; }
        }

        IScriptingSection IUmbracoSettingsSection.Scripting
        {
            get { return Scripting; }
        }
    }
}
