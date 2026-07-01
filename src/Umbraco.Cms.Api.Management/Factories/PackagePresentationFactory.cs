using System.Collections.Specialized;
using System.Web;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class PackagePresentationFactory : IPackagePresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IRuntimeState _runtimeState;
    private readonly MarketplaceSettings _marketplaceSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackagePresentationFactory"/> class.
    /// </summary>
    /// <param name="umbracoMapper">The mapper used for mapping Umbraco objects.</param>
    /// <param name="runtimeState">Provides information about the current runtime state of the Umbraco application.</param>
    /// <param name="marketplaceSettings">A snapshot of the current marketplace settings options.</param>
    public PackagePresentationFactory(IUmbracoMapper umbracoMapper, IRuntimeState runtimeState, IOptionsSnapshot<MarketplaceSettings> marketplaceSettings)
    {
        _umbracoMapper = umbracoMapper;
        _runtimeState = runtimeState;
        _marketplaceSettings = marketplaceSettings.Value;
    }

    /// <summary>
    /// Creates a new <see cref="PackageDefinition"/> instance from the specified <see cref="CreatePackageRequestModel"/>.
    /// </summary>
    /// <param name="createPackageRequestModel">The request model containing the data used to initialize the package definition.</param>
    /// <returns>A <see cref="PackageDefinition"/> populated with values from the provided request model.</returns>
    public PackageDefinition CreatePackageDefinition(CreatePackageRequestModel createPackageRequestModel)
    {
        PackageDefinition packageDefinition = _umbracoMapper.Map<PackageDefinition>(createPackageRequestModel)!;

        // Temp Id, PackageId and PackagePath for the newly created package
        packageDefinition.Id = 0;
        packageDefinition.PackageId = createPackageRequestModel.Id ?? Guid.Empty;
        packageDefinition.PackagePath = string.Empty;

        return packageDefinition;
    }

    /// <summary>
    /// Creates a <see cref="Umbraco.Cms.Api.Management.Models.PackageConfigurationResponseModel" /> instance populated with package configuration data, including the marketplace URL.
    /// </summary>
    /// <returns>A <see cref="Umbraco.Cms.Api.Management.Models.PackageConfigurationResponseModel" /> containing the package's configuration details.</returns>
    public PackageConfigurationResponseModel CreateConfigurationResponseModel() =>
        new()
        {
            MarketplaceUrl = GetMarketplaceUrl(),
        };

    public PagedViewModel<PackageMigrationStatusResponseModel> CreatePackageMigrationStatusResponseModel(PagedModel<InstalledPackage> installedPackages)
    {
        InstalledPackage[] installedPackagesAsArray = installedPackages.Items as InstalledPackage[] ?? installedPackages.Items.ToArray();

        return new PagedViewModel<PackageMigrationStatusResponseModel>
        {
            Total = installedPackages.Total,
            Items = installedPackagesAsArray
                .GroupBy(package => package.PackageName)
                .Select(packages => packages.OrderByDescending(package => package.HasPendingMigrations).First())
                .Select(package => new PackageMigrationStatusResponseModel
                {
                    PackageName = package.PackageName ?? string.Empty,
                    HasPendingMigrations = package.HasPendingMigrations,
                })
                .ToArray(),
        };
    }

    private string GetMarketplaceUrl()
    {
        var uriBuilder = new UriBuilder(Constants.Marketplace.Url);

        NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);

        query["umbversion"] = _runtimeState.SemanticVersion.ToSemanticStringWithoutBuild();
        query["style"] = "backoffice";

        foreach (KeyValuePair<string, string> kvp in _marketplaceSettings.AdditionalParameters)
        {
            query[kvp.Key] = kvp.Value;
        }

        uriBuilder.Query = query.ToString();

        return uriBuilder.ToString();
    }
}
