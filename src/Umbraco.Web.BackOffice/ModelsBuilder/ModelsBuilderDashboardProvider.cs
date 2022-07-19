using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Web.Common.ModelsBuilder;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.ModelsBuilder;

public class ModelsBuilderDashboardProvider : IModelsBuilderDashboardProvider
{
    private readonly LinkGenerator _linkGenerator;

    public ModelsBuilderDashboardProvider(LinkGenerator linkGenerator) => _linkGenerator = linkGenerator;

    public string? GetUrl() =>
        _linkGenerator.GetUmbracoApiServiceBaseUrl<ModelsBuilderDashboardController>(controller =>
            controller.BuildModels());
}
