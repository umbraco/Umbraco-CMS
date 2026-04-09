namespace Umbraco.Cms.Api.Management.ViewModels.Package;

/// <summary>
/// Represents the model returned by the API containing configuration details for a package.
/// </summary>
public class PackageConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets the URL of the package's marketplace page.
    /// </summary>
    public required string MarketplaceUrl { get; set; }
}
