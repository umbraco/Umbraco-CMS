using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

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
