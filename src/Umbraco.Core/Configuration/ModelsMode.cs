namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     Defines the models generation modes.
/// </summary>
public enum ModelsMode
{
    /// <summary>
    ///     Do not generate strongly typed models.
    /// </summary>
    /// <remarks>
    ///     This means that only IPublishedContent instances will be used.
    /// </remarks>
    Nothing = 0,

    /// <summary>
    ///     Generate models in memory.
    ///     When: a content type change occurs.
    /// </summary>
    /// <remarks>The app does not restart. Models are available in views exclusively.</remarks>
    InMemoryAuto,

    /// <summary>
    ///     Generate models as *.cs files.
    ///     When: generation is triggered.
    /// </summary>
    /// <remarks>
    ///     Generation can be triggered from the dashboard. The app does not restart.
    ///     Models are not compiled and thus are not available to the project.
    /// </remarks>
    SourceCodeManual,

    /// <summary>
    ///     Generate models as *.cs files.
    ///     When: a content type change occurs, or generation is triggered.
    /// </summary>
    /// <remarks>
    ///     Generation can be triggered from the dashboard. The app does not restart.
    ///     Models are not compiled and thus are not available to the project.
    /// </remarks>
    SourceCodeAuto,
}
