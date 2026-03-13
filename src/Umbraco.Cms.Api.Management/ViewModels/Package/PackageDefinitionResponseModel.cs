using System.ComponentModel;

namespace Umbraco.Cms.Api.Management.ViewModels.Package;

/// <summary>
/// Represents a response model containing details of a package definition returned by the API.
/// </summary>
public class PackageDefinitionResponseModel : PackageModelBase
{
    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the full path to the package's XML file.
    /// </summary>
    [ReadOnly(true)]
    public required string PackagePath { get; set; }
}
