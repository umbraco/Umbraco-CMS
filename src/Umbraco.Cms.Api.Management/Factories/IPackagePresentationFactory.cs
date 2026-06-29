using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Defines a factory interface for creating presentation models related to packages.
/// </summary>
public interface IPackagePresentationFactory
{
    /// <summary>
    /// Creates a package definition from the provided create package request model.
    /// </summary>
    /// <param name="createPackageRequestModel">The model containing information to create the package definition.</param>
    /// <returns>A <see cref="PackageDefinition"/> representing the created package.</returns>
    PackageDefinition CreatePackageDefinition(CreatePackageRequestModel createPackageRequestModel);

    /// <summary>
    /// Creates and returns a new <see cref="Umbraco.Cms.Api.Management.Models.PackageConfigurationResponseModel"/> instance representing the package configuration response.
    /// </summary>
    /// <returns>The created <see cref="Umbraco.Cms.Api.Management.Models.PackageConfigurationResponseModel"/>.</returns>
    PackageConfigurationResponseModel CreateConfigurationResponseModel();

    /// <summary>
    /// Creates a paged view model of package migration status response models from the given paged installed packages.
    /// </summary>
    /// <param name="installedPackages">The paged model of installed packages to create the migration status response models from.</param>
    /// <returns>A paged view model containing package migration status response models.</returns>
    PagedViewModel<PackageMigrationStatusResponseModel> CreatePackageMigrationStatusResponseModel(PagedModel<InstalledPackage> installedPackages) => new();
}
