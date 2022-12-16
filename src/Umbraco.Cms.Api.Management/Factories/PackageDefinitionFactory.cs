using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal class PackageDefinitionFactory : IPackageDefinitionFactory
{
    private readonly IPackagingService _packagingService;

    public PackageDefinitionFactory(IPackagingService packagingService)
    {
        _packagingService = packagingService;
    }

    public PackageDefinition CreatePackageDefinition(PackageDefinitionViewModel viewModel)
    {
        Guid packageKey = viewModel.Key;

        // Macros are not included!
        var packageDefinition = new PackageDefinition
        {
            PackageId = packageKey,
            Name = viewModel.Name,
            PackagePath = viewModel.PackagePath,
            ContentLoadChildNodes = viewModel.ContentLoadChildNodes,
            ContentNodeId = viewModel.ContentNodeId,
            Languages = viewModel.Languages,
            DictionaryItems = viewModel.DictionaryItems,
            Templates = viewModel.Templates,
            PartialViews = viewModel.PartialViews,
            DocumentTypes = viewModel.DocumentTypes,
            MediaTypes = viewModel.MediaTypes,
            Stylesheets = viewModel.Stylesheets,
            Scripts = viewModel.Scripts,
            DataTypes = viewModel.DataTypes,
            MediaUdis = viewModel.MediaKeys.Select(x => new GuidUdi(Constants.UdiEntityType.Media, x)).ToList(),
            MediaLoadChildNodes = viewModel.MediaLoadChildNodes
        };

        // Trying to see if the package already exists, so we can get its id
        PackageDefinition? package = _packagingService.GetCreatedPackageByKey(packageKey);

        if (package is null)
        {
            // Temp id for the newly created package (the same as an empty package (scaffold))
            packageDefinition.Id = 0;
            packageDefinition.PackageId = Guid.Empty;
            packageDefinition.PackagePath = string.Empty;
        }
        else
        {
            packageDefinition.Id = package.Id;
        }

        return packageDefinition;
    }
}
