using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.ContentApps;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using UserProfile = Umbraco.Cms.Core.Models.ContentEditing.UserProfile;

namespace Umbraco.Cms.Core.Models.Mapping
{
    public class CommonMapper
    {
        private readonly IUserService _userService;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly ContentAppFactoryCollection _contentAppDefinitions;
        private readonly ILocalizedTextService _localizedTextService;

        public CommonMapper(IUserService userService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            ContentAppFactoryCollection contentAppDefinitions, ILocalizedTextService localizedTextService)
        {
            _userService = userService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _contentAppDefinitions = contentAppDefinitions;
            _localizedTextService = localizedTextService;
        }

        public UserProfile? GetOwner(IContentBase source, MapperContext context)
        {
            var profile = source.GetCreatorProfile(_userService);
            return profile == null ? null : context.Map<IProfile, UserProfile>(profile);
        }

        public UserProfile? GetCreator(IContent source, MapperContext context)
        {
            var profile = source.GetWriterProfile(_userService);
            return profile == null ? null : context.Map<IProfile, UserProfile>(profile);
        }

        public ContentTypeBasic? GetContentType(IContentBase source, MapperContext context)
        {
            var contentType = _contentTypeBaseServiceProvider.GetContentTypeOf(source);
            var contentTypeBasic = context.Map<IContentTypeComposition, ContentTypeBasic>(contentType);
            return contentTypeBasic;
        }

        public IEnumerable<ContentApp> GetContentApps(IUmbracoEntity source)
        {
            return GetContentAppsForEntity(source);
        }

        public IEnumerable<ContentApp> GetContentAppsForEntity(IEntity source)
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
