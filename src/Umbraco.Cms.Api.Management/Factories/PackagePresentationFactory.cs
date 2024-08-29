using System.Collections.Specialized;
using System.Web;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal class PackagePresentationFactory : IPackagePresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IRuntimeState _runtimeState;
    private readonly MarketplaceSettings _marketplaceSettings;

    public PackagePresentationFactory(IUmbracoMapper umbracoMapper, IRuntimeState runtimeState, IOptionsSnapshot<MarketplaceSettings> marketplaceSettings)
    {
        _umbracoMapper = umbracoMapper;
        _runtimeState = runtimeState;
        _marketplaceSettings = marketplaceSettings.Value;
    }

    public PackageDefinition CreatePackageDefinition(CreatePackageRequestModel createPackageRequestModel)
    {
        PackageDefinition packageDefinition = _umbracoMapper.Map<PackageDefinition>(createPackageRequestModel)!;

        // Temp Id, PackageId and PackagePath for the newly created package
        packageDefinition.Id = 0;
        packageDefinition.PackageId = createPackageRequestModel.Id ?? Guid.Empty;
        packageDefinition.PackagePath = string.Empty;

        return packageDefinition;
    }

    public PackageConfigurationResponseModel CreateConfigurationResponseModel() =>
        new()
        {
            MarketplaceUrl = GetMarketplaceUrl(),
        };

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
