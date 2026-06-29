namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Represents the response model returned from an Umbraco server upgrade check.
/// </summary>
[Obsolete("Upgrade checks are no longer supported. Scheduled for removal in Umbraco 19.")]
public class UpgradeCheckResponseModel
{
    /// <summary>
    /// Gets or sets the category or classification of the upgrade check response.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// A comment providing additional information about the upgrade check response.
    /// </summary>
    public required string Comment { get; init; }

    /// <summary>
    /// Gets or sets the URL that provides additional information or actions related to the upgrade check response.
    /// </summary>
    public required string Url { get; init; }
}
