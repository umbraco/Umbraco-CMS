namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

/// <summary>
/// Represents the default response model returned when retrieving a tracked reference in the API.
/// </summary>
public class DefaultReferenceResponseModel : ReferenceResponseModel
{
    /// <summary>
    /// Gets or sets the type of the tracked reference, indicating the category or kind of referenced entity.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the icon representing the reference.
    /// </summary>
    public string? Icon { get; set; }
}
