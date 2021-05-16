using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Provides extensions for the <see cref="ModelsMode"/> enumeration.
    /// </summary>
    public static class ModelsModeExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the mode is LiveAnything or Runtime.
        /// </summary>
        public static bool IsLive(this ModelsMode modelsMode)
            => modelsMode == ModelsMode.Runtime || modelsMode == ModelsMode.LiveCode;

        /// <summary>
        /// Gets a value indicating whether the mode is LiveAnything but not Runtime.
        /// </summary>
        public static bool IsLiveNotRuntime(this ModelsMode modelsMode)
            => modelsMode == ModelsMode.LiveCode;

        /// <summary>
        /// Gets a value indicating whether the mode supports explicit manual generation.
        /// </summary>
        public static bool SupportsExplicitGeneration(this ModelsMode modelsMode)
            => modelsMode == ModelsMode.Code || modelsMode == ModelsMode.LiveCode;
    }
}
