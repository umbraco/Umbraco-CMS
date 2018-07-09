using System;
using System.ComponentModel;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IUmbracoSettingsSection : IUmbracoConfigurationSection
    {
        IBackOfficeSection BackOffice { get; }

        IContentSection Content { get; }

        ISecuritySection Security { get; }

        IRequestHandlerSection RequestHandler { get; }

        ITemplatesSection Templates { get; }

        IDeveloperSection Developer { get; }

        IViewStateMoverModuleSection ViewStateMoverModule { get; }

        ILoggingSection Logging { get; }

        IScheduledTasksSection ScheduledTasks { get; }

        IDistributedCallSection DistributedCall { get; }
        
        IProvidersSection Providers { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This is no longer used and will be removed in future versions")]
        IHelpSection Help { get; }

        IWebRoutingSection WebRouting { get; }

        IScriptingSection Scripting { get; }
    }
}
