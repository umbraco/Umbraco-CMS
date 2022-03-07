using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IConfiguration" />.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Determines whether the configuration value is set to the specified runtime mode.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="runtimeMode">The runtime mode.</param>
        /// <returns>
        ///   <c>true</c> if the configuration value is set to the specified runtime mode; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRuntimeMode(this IConfiguration configuration, RuntimeMode runtimeMode)
            => configuration.GetValue<RuntimeMode>(Cms.Core.Constants.Configuration.ConfigRuntimeMode) == runtimeMode;
    }
}
