using System.Web.Mvc;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Gets the tree node url for the content or media
    /// </summary>
    internal class ContentTreeNodeUrlResolver<TSource, TController> : IValueResolver<TSource, object, string>
        where TSource : IContentBase
        where TController : ContentTreeControllerBase
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public ContentTreeNodeUrlResolver(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new System.ArgumentNullException(nameof(umbracoContextAccessor));
        }

        public string Resolve(TSource source, object destination, string destMember, ResolutionContext context)
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (umbracoContext == null) return null;

            var urlHelper = new UrlHelper(umbracoContext.HttpContext.Request.RequestContext);
            return urlHelper.GetUmbracoApiService<TController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null));
        }
    }
}
