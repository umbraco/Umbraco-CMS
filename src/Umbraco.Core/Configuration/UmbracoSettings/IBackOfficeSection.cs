namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IBackOfficeSection
    {
        ITourSection Tours { get; }

        IServiceWorkerSection ServiceWorker { get; }
    }
}