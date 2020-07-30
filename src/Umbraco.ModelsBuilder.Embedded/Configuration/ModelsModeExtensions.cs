﻿namespace Umbraco.ModelsBuilder.Embedded.Configuration
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
        {
            return
                modelsMode == ModelsMode.PureLive
                || modelsMode == ModelsMode.LiveAppData;
        }

        /// <summary>
        /// Gets a value indicating whether the mode is LiveAnything but not PureLive.
        /// </summary>
        public static bool IsLiveNotPure(this ModelsMode modelsMode)
        {
            return
                modelsMode == ModelsMode.LiveAppData;
        }

        /// <summary>
        /// Gets a value indicating whether the mode supports explicit generation (as opposed to pure live).
        /// </summary>
        public static bool SupportsExplicitGeneration(this ModelsMode modelsMode)
        {
            return 
               modelsMode == ModelsMode.AppData
               || modelsMode == ModelsMode.LiveAppData;
        }
    }
}
