using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Core.Features;

/// <summary>
///     Represents disabled features.
/// </summary>
public class DisabledFeatures
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DisabledFeatures" /> class.
    /// </summary>
    public DisabledFeatures() => Controllers = new TypeList<IUmbracoFeature>();

    /// <summary>
    ///     Gets the disabled controllers.
    /// </summary>
    public TypeList<IUmbracoFeature> Controllers { get; }

    /// <summary>
    ///     Disables the device preview feature of previewing.
    /// </summary>
    public bool DisableDevicePreview { get; set; }

    /// <summary>
    ///     If true, all references to templates will be removed in the back office and routing
    /// </summary>
    public bool DisableTemplates { get; set; }
}
