namespace Umbraco.Core.Hosting
{
    internal class NoopUmbracoApplicationLifetimeManager : IUmbracoApplicationLifetimeManager
    {
        public void InvokeApplicationInit() { }
    }
}
