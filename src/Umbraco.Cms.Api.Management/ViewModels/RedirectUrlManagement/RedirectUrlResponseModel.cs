namespace Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

/// <summary>
/// Represents a response model containing information about a redirect URL, typically used in redirect URL management API responses.
/// </summary>
public class RedirectUrlResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier for this redirect URL.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the original URL before redirection.
    /// </summary>
    public required string OriginalUrl { get; set; }

    /// <summary>Gets or sets the destination URL for the redirect.</summary>
    public required string DestinationUrl { get; set; }

    /// <summary>Gets or sets the creation date and time of the redirect URL.</summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Gets or sets a reference to the document that is the target of the redirect URL.
    /// </summary>
    public required ReferenceByIdModel Document { get; set; }

    /// <summary>
    /// Gets or sets the culture associated with the redirect URL.
    /// </summary>
    public string? Culture { get; set; }
}
