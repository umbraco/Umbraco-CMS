using Umbraco.Core.Configuration;
using Umbraco.ModelsBuilder.Configuration;

namespace Umbraco.ModelsBuilder
{
    /// <summary>
    /// Provides extension methods for the <see cref="UmbracoConfig"/> class.
    /// </summary>
    public static class ConfigsExtensions
    {
        /// <summary>
        /// Gets the models builder configuration.
        /// </summary>
        /// <remarks>Getting the models builder configuration freezes its state,
        /// and any attempt at modifying the configuration using the Setup method
        /// will be ignored.</remarks>
        public static Config ModelsBuilder(this Configs configs)
            => configs.GetConfig<Config>();
    }
}