using Microsoft.AspNetCore.Routing;
using Umbraco.Extensions;
using Umbraco.Web.Common.ModelsBuilder;

namespace Umbraco.Web.BackOffice.ModelsBuilder
{
    public class ModelsBuilderDashboardProvider: IModelsBuilderDashboardProvider
    {
        private readonly LinkGenerator _linkGenerator;

        public ModelsBuilderDashboardProvider(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public string GetUrl() =>
            _linkGenerator.GetUmbracoApiServiceBaseUrl<ModelsBuilderDashboardController>(controller =>
                controller.BuildModels());
    }
}
