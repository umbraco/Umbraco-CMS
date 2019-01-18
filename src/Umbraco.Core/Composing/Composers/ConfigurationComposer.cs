using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Composing.Composers
{
    /// <summary>
    /// Compose configurations.
    /// </summary>
    public static class ConfigurationComposer
    {
        public static Composition ComposeConfiguration(this Composition composition)
        {
            // common configurations are already registered
            // register others

            composition.RegisterUnique(factory => factory.GetInstance<IUmbracoSettingsSection>().Content);
            composition.RegisterUnique(factory => factory.GetInstance<IUmbracoSettingsSection>().RequestHandler);
            composition.RegisterUnique(factory => factory.GetInstance<IUmbracoSettingsSection>().Security);

            return composition;
        }
    }
}
