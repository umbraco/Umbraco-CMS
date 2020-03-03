using Umbraco.Core.Configuration;
using Umbraco.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Composing.CompositionExtensions
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

            composition.RegisterUnique<IIndexCreatorSettings, IndexCreatorSettings>();
            composition.RegisterUnique<INuCacheSettings, NuCacheSettings>();
            composition.RegisterUnique<ITypeFinderSettings, TypeFinderSettings>();
            composition.RegisterUnique<IRuntimeSettings, RuntimeSettings>();
            composition.RegisterUnique<IActiveDirectorySettings, IActiveDirectorySettings>();
            composition.RegisterUnique<IExceptionFilterSettings, ExceptionFilterSettings>();
            composition.RegisterUnique<IModelsBuilderConfig, ModelsBuilderConfig>();

            return composition;
        }
    }
}
