namespace Umbraco.Cms.Core.Web.Mvc;

/// <summary>
///     Represents some metadata about the controller
/// </summary>
public class PluginControllerMetadata
{
    public Type ControllerType { get; set; } = null!;

    public string? ControllerName { get; set; }

    public string? ControllerNamespace { get; set; }

    public string? AreaName { get; set; }

    /// <summary>
    ///     This is determined by another attribute [IsBackOffice] which slightly modifies the route path
    ///     allowing us to determine if it is indeed a back office request or not
    /// </summary>
    public bool IsBackOffice { get; set; }
}
