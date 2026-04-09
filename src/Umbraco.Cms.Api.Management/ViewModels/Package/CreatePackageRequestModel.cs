namespace Umbraco.Cms.Api.Management.ViewModels.Package;

/// <summary>
/// Represents the data required to create a new package via the API.
/// </summary>
public class CreatePackageRequestModel : PackageModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the package.
    /// </summary>
    public Guid? Id { get; set; }
}
