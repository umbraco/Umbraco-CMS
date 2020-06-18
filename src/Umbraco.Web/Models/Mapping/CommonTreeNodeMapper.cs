using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
//Migrated to .NET CORE
namespace Umbraco.Web.Models.Mapping
{
    public class CommonTreeNodeMapper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;


        public CommonTreeNodeMapper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public string GetTreeNodeUrl<TController>(IContentBase source)
            where TController : UmbracoApiController, ITreeNodeController
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            var urlHelper = new UrlHelper(httpContext.Request.RequestContext);
            return urlHelper.GetUmbracoApiService<TController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null));
        }

    }
}
