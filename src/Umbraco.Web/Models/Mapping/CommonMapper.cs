using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

namespace Umbraco.Web.Models.Mapping
{
    internal class CommonMapper
    {
        private readonly IUserService _userService;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ContentAppFactoryCollection _contentAppDefinitions;
        private readonly ILocalizedTextService _localizedTextService;

        public CommonMapper(IUserService userService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, IUmbracoContextAccessor umbracoContextAccessor,
            ContentAppFactoryCollection contentAppDefinitions, ILocalizedTextService localizedTextService)
        {
            _userService = userService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
            _contentAppDefinitions = contentAppDefinitions;
            _localizedTextService = localizedTextService;
        }

        public UserProfile GetOwner(IContentBase source, MapperContext context)
        {
            var profile = source.GetCreatorProfile(_userService);
            return profile == null ? null : context.Map<IProfile, UserProfile>(profile);
        }

        public UserProfile GetCreator(IContent source, MapperContext context)
        {
            var profile = source.GetWriterProfile(_userService);
            return profile == null ? null : context.Map<IProfile, UserProfile>(profile);
        }

        public ContentTypeBasic GetContentType(IContentBase source, MapperContext context)
        {
            var contentType = _contentTypeBaseServiceProvider.GetContentTypeOf(source);
            var contentTypeBasic = context.Map<IContentTypeComposition, ContentTypeBasic>(contentType);
            return contentTypeBasic;
        }

        public string GetTreeNodeUrl<TController>(IContentBase source)
            where TController : ContentTreeControllerBase
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (umbracoContext == null) return null;

            var urlHelper = new UrlHelper(umbracoContext.HttpContext.Request.RequestContext);
            return urlHelper.GetUmbracoApiService<TController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null));
        }

        public string GetMemberTreeNodeUrl(IContentBase source)
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (umbracoContext == null) return null;

            var urlHelper = new UrlHelper(umbracoContext.HttpContext.Request.RequestContext);
            return urlHelper.GetUmbracoApiService<MemberTreeController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null));
        }

        public IEnumerable<ContentApp> GetContentApps(IUmbracoEntity source)
        {
            var apps = _contentAppDefinitions.GetContentAppsFor(source).ToArray();

            // localize content app names
            foreach (var app in apps)
            {
                var localizedAppName = _localizedTextService.Localize("apps", app.Alias);
                if (localizedAppName.Equals($"[{app.Alias}]", StringComparison.OrdinalIgnoreCase) == false)
                {
                    app.Name = localizedAppName;
                }
            }

            return apps;
        }
    }
}
