using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Provides extensions for the <see cref="ModelsMode"/> enumeration.
    /// </summary>
    public static class ModelsModeExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the mode is LiveAnything or PureLive.
        /// </summary>
        public static bool IsLive(this ModelsMode modelsMode)
            => modelsMode == ModelsMode.PureLive || modelsMode == ModelsMode.LiveAppData;

        /// <summary>
        /// Gets a value indicating whether the mode is LiveAnything but not PureLive.
        /// </summary>
        public static bool IsLiveNotPure(this ModelsMode modelsMode)
            => modelsMode == ModelsMode.LiveAppData;

        /// <summary>
        /// Gets a value indicating whether the mode supports explicit generation (as opposed to pure live).
        /// </summary>
        public static bool SupportsExplicitGeneration(this ModelsMode modelsMode)
            => modelsMode == ModelsMode.AppData || modelsMode == ModelsMode.LiveAppData;
    }
}
