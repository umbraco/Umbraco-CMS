using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IPackagePresentationFactory
{
    PackageDefinition CreatePackageDefinition(CreatePackageRequestModel createPackageRequestModel);

    PackageConfigurationResponseModel CreateConfigurationResponseModel();
}
