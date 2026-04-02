namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the response model returned by the Management API for document configuration settings.
/// </summary>
public class DocumentConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether deleting the document is disabled when it is referenced by other entities.
    /// </summary>
    public required bool DisableDeleteWhenReferenced { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether unpublishing is disabled when the document is referenced.
    /// </summary>
    public required bool DisableUnpublishWhenReferenced { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether editing the invariant language is allowed from a non-default language.
    /// </summary>
    public required bool AllowEditInvariantFromNonDefault { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the creation of segments that do not already exist is permitted for this document configuration.
    /// </summary>
    [Obsolete("This functionality will be moved to a client-side extension. Scheduled for removal in Umbraco 19.")]
    public required bool AllowNonExistingSegmentsCreation { get; set; }
}
