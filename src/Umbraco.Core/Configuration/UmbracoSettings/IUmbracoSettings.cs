namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IUmbracoSettings
    {
        IContent Content { get; }

        ISecurity Security { get; }

        IRequestHandler RequestHandler { get; }

        ITemplates Templates { get; }

        IDeveloper Developer { get; }

        IViewstateMoverModule ViewstateMoverModule { get; }

        ILogging Logging { get; }

        IScheduledTasks ScheduledTasks { get; }

        IDistributedCall DistributedCall { get; }

        IRepositories PackageRepositories { get; }

        IProviders Providers { get; }

        IHelp Help { get; }

        IWebRouting WebRouting { get; }

        IScripting Scripting { get; }
    }
}