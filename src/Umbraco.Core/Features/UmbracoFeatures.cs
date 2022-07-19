namespace Umbraco.Cms.Core.Features;

/// <summary>
///     Represents the Umbraco features.
/// </summary>
public class UmbracoFeatures
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoFeatures" /> class.
    /// </summary>
    public UmbracoFeatures()
    {
        Disabled = new DisabledFeatures();
        Enabled = new EnabledFeatures();
    }

    /// <summary>
    ///     Gets the disabled features.
    /// </summary>
    public DisabledFeatures Disabled { get; }

    /// <summary>
    ///     Gets the enabled features.
    /// </summary>
    public EnabledFeatures Enabled { get; }

    /// <summary>
    ///     Determines whether a controller is enabled.
    /// </summary>
    public bool IsControllerEnabled(Type? feature)
    {
        if (typeof(IUmbracoFeature).IsAssignableFrom(feature))
        {
            return Disabled.Controllers.Contains(feature) == false;
        }

        throw new NotSupportedException("Not a supported feature type.");
    }
}
