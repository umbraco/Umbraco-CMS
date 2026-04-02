namespace Umbraco.Cms.Core.Web.Mvc;

/// <summary>
/// Represents metadata about a plugin controller.
/// </summary>
public class PluginControllerMetadata
{
    /// <summary>
    /// Gets or sets the type of the controller.
    /// </summary>
    public Type ControllerType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the controller.
    /// </summary>
    public string? ControllerName { get; set; }

    /// <summary>
    /// Gets or sets the namespace of the controller.
    /// </summary>
    public string? ControllerNamespace { get; set; }

    /// <summary>
    /// Gets or sets the area name for the controller.
    /// </summary>
    public string? AreaName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this controller is a back office controller.
    /// </summary>
    /// <remarks>
    /// This is determined by another attribute [IsBackOffice] which slightly modifies the route path
    /// allowing us to determine if it is indeed a back office request or not.
    /// </remarks>
    public bool IsBackOffice { get; set; }
}
