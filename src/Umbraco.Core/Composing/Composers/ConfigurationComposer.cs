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
            composition.Register(factory => UmbracoConfig.For.UmbracoSettings());
            composition.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().Content);
            composition.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().Templates);
            composition.Register(factory => factory.GetInstance<IUmbracoSettingsSection>().RequestHandler);
            composition.Register(factory => UmbracoConfig.For.GlobalSettings());

            // fixme - other sections we need to add?

            return composition;
        }
    }
}
