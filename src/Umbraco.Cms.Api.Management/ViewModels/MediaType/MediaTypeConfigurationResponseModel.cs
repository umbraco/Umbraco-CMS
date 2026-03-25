namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

/// <summary>
/// Represents a response model containing configuration details for a media type in the Umbraco CMS management API.
/// </summary>
public class MediaTypeConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets the collection of field names that are reserved and cannot be used for custom properties in the media type configuration.
    /// </summary>
    public required ISet<string> ReservedFieldNames { get; set; }
}
