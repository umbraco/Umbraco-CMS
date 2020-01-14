namespace Umbraco.ModelsBuilder.Embedded.Configuration
{
    /// <summary>
    /// Defines the models generation modes.
    /// </summary>
    public enum ModelsMode
    {
        /// <summary>
        /// Do not generate models.
        /// </summary>
        Nothing = 0, // default value 

        /// <summary>
        /// Generate models in memory.
        /// When: a content type change occurs.
        /// </summary>
        /// <remarks>The app does not restart. Models are available in views exclusively.</remarks>
        PureLive,

        /// <summary>
        /// Generate models in AppData.
        /// When: generation is triggered.
        /// </summary>
        /// <remarks>Generation can be triggered from the dashboard. The app does not restart.
        /// Models are not compiled and thus are not available to the project.</remarks>
        AppData,

        /// <summary>
        /// Generate models in AppData.
        /// When: a content type change occurs, or generation is triggered.
        /// </summary>
        /// <remarks>Generation can be triggered from the dashboard. The app does not restart.
        /// Models are not compiled and thus are not available to the project.</remarks>
        LiveAppData
    }
}
