using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IPackagePresentationFactory
{
    PackageDefinition CreatePackageDefinition(CreatePackageRequestModel createPackageRequestModel);

    PackageConfigurationResponseModel CreateConfigurationResponseModel();

    PagedViewModel<PackageMigrationStatusResponseModel> CreatePackageMigrationStatusResponseModel(PagedModel<InstalledPackage> installedPackages) => new();
}
