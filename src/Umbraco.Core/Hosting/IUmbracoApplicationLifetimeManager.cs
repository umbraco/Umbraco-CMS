namespace Umbraco.Core.Hosting
{
    // TODO: Should be killed and replaced with UmbracoApplicationStarting notifications
    public interface IUmbracoApplicationLifetimeManager
    {
        void InvokeApplicationInit();
    }
}
