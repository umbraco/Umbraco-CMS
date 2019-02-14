using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Composing.CompositionExtensions
{
    /// <summary>
    /// Compose configurations.
    /// </summary>
    public static class Configuration
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
