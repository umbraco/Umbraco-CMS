using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Mapping
{
    public class CommonTreeNodeMapper
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IHostingEnvironment _hostingEnvironment;


        public CommonTreeNodeMapper(LinkGenerator linkGenerator, IHostingEnvironment hostingEnvironment)
        {
            _linkGenerator = linkGenerator;
            _hostingEnvironment = hostingEnvironment;
        }


        public string GetTreeNodeUrl<TController>(IContentBase source)
            where TController : UmbracoApiController, ITreeNodeController
        {
            return _linkGenerator.GetUmbracoApiService<TController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null), this._hostingEnvironment.ApplicationVirtualPath);
        }

    }
}
