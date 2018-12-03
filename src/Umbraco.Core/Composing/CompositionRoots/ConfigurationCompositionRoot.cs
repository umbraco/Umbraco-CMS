using LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Composing.CompositionRoots
{
    /// <summary>
    /// Sets up IoC container for Umbraco configuration classes
    /// </summary>
    public sealed class ConfigurationCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry container)
        {
            container.Register(factory => UmbracoConfig.For.UmbracoSettings());
            container.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().Content);
            container.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().Templates);
            container.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().RequestHandler);
            container.Register(factory => UmbracoConfig.For.GlobalSettings());
            container.Register(factory => UmbracoConfig.For.DashboardSettings());

            // fixme - other sections we need to add?
        }
    }
}
