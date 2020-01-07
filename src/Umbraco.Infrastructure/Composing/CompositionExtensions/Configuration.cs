using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.Composing.CompositionExtensions
{
    /// <summary>
    /// Compose configurations.
    /// </summary>
    internal static class Configuration
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
