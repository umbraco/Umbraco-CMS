using LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Sets up IoC container for Umbraco configuration classes
    /// </summary>
    public sealed class ConfigurationCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<IUmbracoSettingsSection>(factory => UmbracoConfig.For.UmbracoSettings());
            serviceRegistry.Register<IContentSection>(factory => factory.GetInstance<IUmbracoSettingsSection>().Content);
            serviceRegistry.Register<ITemplatesSection>(factory => factory.GetInstance<IUmbracoSettingsSection>().Templates);
            serviceRegistry.Register<IRequestHandlerSection>(factory => factory.GetInstance<IUmbracoSettingsSection>().RequestHandler);

            //TODO: Add the other config areas...
        }
    }
}