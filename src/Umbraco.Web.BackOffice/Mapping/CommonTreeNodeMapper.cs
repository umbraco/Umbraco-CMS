using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Umbraco.Core.Models;
using Umbraco.Extensions;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    public class CommonTreeNodeMapper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;


        public CommonTreeNodeMapper(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }


        public string GetTreeNodeUrl<TController>(IContentBase source)
            where TController : UmbracoApiController, ITreeNodeController
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;


            return _linkGenerator.GetUmbracoApiService<TController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null));
        }

    }
}
