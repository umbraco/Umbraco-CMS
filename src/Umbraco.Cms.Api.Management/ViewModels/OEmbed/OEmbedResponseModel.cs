namespace Umbraco.Cms.Api.Management.ViewModels.OEmbed;

/// <summary>
/// Represents the response model for an OEmbed request.
/// </summary>
public class OEmbedResponseModel
{
    /// <summary>
    /// Gets or sets the HTML markup for the embedded content.
    /// </summary>
    public required string Markup { get; set; }
}
