namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

/// <summary>
/// Represents the data required to import a media type via an API request.
/// </summary>
public class ImportMediaTypeRequestModel
{
    /// <summary>
    /// Gets or sets the reference to the file containing the media type definition to import.
    /// </summary>
    public required ReferenceByIdModel File { get; set; }
}
