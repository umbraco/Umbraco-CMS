using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extensions for <see cref="RuntimeSettings" />.
    /// </summary>
    public static class RuntimeSettingsExtensions
    {
        /// <summary>
        /// Determines whether the runtime mode is set to development.
        /// </summary>
        /// <param name="runtimeSettings">The runtime settings.</param>
        /// <returns>
        ///   <c>true</c> if the runtime mode is set to development; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDevelopment(this RuntimeSettings runtimeSettings)
            => runtimeSettings.Mode == RuntimeMode.Development;

        /// <summary>
        /// Determines whether the runtime mode is set to production.
        /// </summary>
        /// <param name="runtimeSettings">The runtime settings.</param>
        /// <returns>
        ///   <c>true</c> if the runtime mode is set to production; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsProduction(this RuntimeSettings runtimeSettings)
            => runtimeSettings.Mode == RuntimeMode.Production;
    }
}
