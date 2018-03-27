using System.Web.Mvc;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Gets the tree node url for the IMember
    /// </summary>
    internal class MemberTreeNodeUrlResolver : IValueResolver<IMember, MemberDisplay, string>
    {
        public string Resolve(IMember source, MemberDisplay destination, string destMember, ResolutionContext context)
        {
            var umbracoContext = context.GetUmbracoContext(throwIfMissing: false);
            if (umbracoContext == null) return null;

            var urlHelper = new UrlHelper(umbracoContext.HttpContext.Request.RequestContext);
            return urlHelper.GetUmbracoApiService<MemberTreeController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null));
        }
    }
}
