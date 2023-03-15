using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IPackageDefinitionFactory
{
    PackageDefinition CreatePackageDefinition(CreatePackageRequestModel createPackageRequestModel);
}
