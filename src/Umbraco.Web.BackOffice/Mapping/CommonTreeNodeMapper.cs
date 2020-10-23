using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Umbraco.Core.Models;
using Umbraco.Extensions;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.BackOffice.Trees;

namespace Umbraco.Web.BackOffice.Mapping
{
    public class CommonTreeNodeMapper
    {
        private readonly LinkGenerator _linkGenerator;


        public CommonTreeNodeMapper( LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }


        public string GetTreeNodeUrl<TController>(IContentBase source)
            where TController : UmbracoApiController, ITreeNodeController
        {
            return _linkGenerator.GetUmbracoApiService<TController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null));
        }

    }
}
