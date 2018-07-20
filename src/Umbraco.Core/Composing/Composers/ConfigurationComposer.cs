using LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Composing.Composers
{
    /// <summary>
    /// Sets up IoC container for Umbraco configuration classes
    /// </summary>
    public static class ConfigurationComposer
    {
        public static IServiceRegistry ComposeConfiguration(this IServiceRegistry registry)
        {
            registry.Register(factory => UmbracoConfig.For.UmbracoSettings());
            registry.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().Content);
            registry.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().Templates);
            registry.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().RequestHandler);
            registry.Register(factory => UmbracoConfig.For.GlobalSettings());

            // fixme - other sections we need to add?

            return registry;
        }
    }
}
