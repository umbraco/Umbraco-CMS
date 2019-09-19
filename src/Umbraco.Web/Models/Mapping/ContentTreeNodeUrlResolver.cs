using System.Web.Mvc;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Gets the tree node url for the content or media
    /// </summary>
    internal class ContentTreeNodeUrlResolver<TSource, TController> : IValueResolver
        where TSource : IContentBase
        where TController : ContentTreeControllerBase
    {

        public ResolutionResult Resolve(ResolutionResult source)
        {
            return source.New(ResolveCore(source, (TSource)source.Value), typeof(string));
        }

        private string ResolveCore(ResolutionResult res, TSource source)
        {
            var umbCtx = res.Context.GetUmbracoContext();
            //map the tree node url
            if (umbCtx != null)
            {
                var urlHelper = new UrlHelper(umbCtx.HttpContext.Request.RequestContext);
                var url = urlHelper.GetUmbracoApiService<TController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null));
                return url;
            }
            return null;
        }
    }
}
