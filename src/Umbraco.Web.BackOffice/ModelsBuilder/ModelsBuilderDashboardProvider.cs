using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Web.Common.ModelsBuilder;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.ModelsBuilder
{
    public class ModelsBuilderDashboardProvider: IModelsBuilderDashboardProvider
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ModelsBuilderDashboardProvider(LinkGenerator linkGenerator, IHostingEnvironment hostingEnvironment)
        {
            _linkGenerator = linkGenerator;
            _hostingEnvironment = hostingEnvironment;
        }

        public string GetUrl() =>
            _linkGenerator.GetUmbracoApiServiceBaseUrl<ModelsBuilderDashboardController>(controller =>
                controller.BuildModels(), this._hostingEnvironment.ApplicationVirtualPath);
    }
}
