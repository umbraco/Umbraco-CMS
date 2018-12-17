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
            composition.RegisterUnique<UmbracoConfig>();
            composition.RegisterUnique(factory => factory.GetInstance<UmbracoConfig>().Umbraco());
            composition.RegisterUnique(factory => factory.GetInstance<IUmbracoSettingsSection>().Content);
            composition.RegisterUnique(factory => factory.GetInstance<IUmbracoSettingsSection>().Templates);
            composition.RegisterUnique(factory => factory.GetInstance<IUmbracoSettingsSection>().RequestHandler);
            composition.RegisterUnique(factory => factory.GetInstance<IUmbracoSettingsSection>().Security);
            composition.RegisterUnique(factory => factory.GetInstance<UmbracoConfig>().Global());
            composition.RegisterUnique(factory => factory.GetInstance<UmbracoConfig>().Dashboards());
            composition.RegisterUnique(factory => factory.GetInstance<UmbracoConfig>().HealthChecks());
            composition.RegisterUnique(factory => factory.GetInstance<UmbracoConfig>().Grids());

            // fixme - other sections we need to add?

            return composition;
        }
    }
}
