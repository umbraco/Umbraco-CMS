using System.Web.Mvc;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Gets the tree node url for the IMember
    /// </summary>
    internal class MemberTreeNodeUrlResolver : IValueResolver
    {
        
        public ResolutionResult Resolve(ResolutionResult source)
        {
            return source.New(ResolveCore(source, (IMember)source.Value), typeof(string));
        }

        private string ResolveCore(ResolutionResult res, IMember source)
        {
            var umbCtx = res.Context.GetUmbracoContext();
            //map the tree node url
            if (umbCtx != null)
            {
                var urlHelper = new UrlHelper(umbCtx.HttpContext.Request.RequestContext);
                var url = urlHelper.GetUmbracoApiService<MemberTreeController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null));
                return url;
            }
            return null;
        }
    }
}
