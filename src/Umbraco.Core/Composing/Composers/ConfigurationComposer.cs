using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Composing.Composers
{
    /// <summary>
    /// Sets up IoC container for Umbraco configuration classes
    /// </summary>
    public static class ConfigurationComposer
    {
        public static Composition ComposeConfiguration(this Composition composition)
        {
            var container = composition.Container;

            container.Register(factory => UmbracoConfig.For.UmbracoSettings());
            container.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().Content);
            container.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().Templates);
            container.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().RequestHandler);
            container.Register(factory => UmbracoConfig.For.GlobalSettings());

            // fixme - other sections we need to add?

            return composition;
        }
    }
}
