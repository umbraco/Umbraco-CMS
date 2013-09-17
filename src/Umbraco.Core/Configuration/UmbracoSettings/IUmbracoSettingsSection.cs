namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IUmbracoSettingsSection : IUmbracoConfigurationSection
    {
        IContentSection Content { get; }

        ISecuritySection Security { get; }

        IRequestHandlerSection RequestHandler { get; }

        ITemplatesSection Templates { get; }

        IDeveloperSection Developer { get; }

        IViewStateMoverModuleSection ViewStateMoverModule { get; }

        ILoggingSection Logging { get; }

        IScheduledTasksSection ScheduledTasks { get; }

        IDistributedCallSection DistributedCall { get; }

        IRepositoriesSection PackageRepositories { get; }

        IProvidersSection Providers { get; }

        IHelpSection Help { get; }

        IWebRoutingSection WebRouting { get; }

        IScriptingSection Scripting { get; }
    }
}