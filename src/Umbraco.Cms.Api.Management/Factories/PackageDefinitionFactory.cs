using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Api.Management.Factories;

internal class PackageDefinitionFactory : IPackageDefinitionFactory
{
    private readonly IUmbracoMapper _umbracoMapper;

    public PackageDefinitionFactory(IUmbracoMapper umbracoMapper) => _umbracoMapper = umbracoMapper;

    public PackageDefinition CreatePackageDefinition(PackageCreateModel packageCreateModel)
    {
        // Macros are not included!
        PackageDefinition packageDefinition = _umbracoMapper.Map<PackageDefinition>(packageCreateModel)!;

        // Temp Id, PackageId and PackagePath for the newly created package
        packageDefinition.Id = 0;
        packageDefinition.PackageId = Guid.Empty;
        packageDefinition.PackagePath = string.Empty;

        return packageDefinition;
    }
}
