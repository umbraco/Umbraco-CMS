namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the response model returned by the Management API for element configuration settings.
/// </summary>
public class ElementConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether deleting the element is disabled when it is referenced by other entities.
    /// </summary>
    public required bool DisableDeleteWhenReferenced { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether unpublishing is disabled when the element is referenced.
    /// </summary>
    public required bool DisableUnpublishWhenReferenced { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether editing the invariant language is allowed from a non-default language.
    /// </summary>
    public required bool AllowEditInvariantFromNonDefault { get; set; }
}
