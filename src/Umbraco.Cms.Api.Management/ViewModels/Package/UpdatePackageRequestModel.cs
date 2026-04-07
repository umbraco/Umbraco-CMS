using System.ComponentModel;

namespace Umbraco.Cms.Api.Management.ViewModels.Package;

/// <summary>
/// Represents the API request model used for updating a package in the management system.
/// </summary>
public class UpdatePackageRequestModel : PackageModelBase
{
    /// <summary>
    ///     Gets or sets the full path to the package's XML file.
    /// </summary>
    [ReadOnly(true)]
    public required string PackagePath { get; set; }
}
