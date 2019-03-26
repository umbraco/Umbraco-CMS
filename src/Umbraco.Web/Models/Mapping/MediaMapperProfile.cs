using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for media.
    /// </summary>
    internal class MediaMapperProfile : IMapperProfile
    {
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly ContentAppFactoryCollection _contentAppDefinitions;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly TabsAndPropertiesMapper<IMedia> _tabsAndPropertiesMapper;

        public MediaMapperProfile(ILogger logger, IUserService userService, IMediaService mediaService, IMediaTypeService mediaTypeService,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, ContentAppFactoryCollection contentAppDefinitions,
            IUmbracoContextAccessor umbracoContextAccessor, ILocalizedTextService localizedTextService)
        {
            _logger = logger;
            _userService = userService;
            _mediaService = mediaService;
            _mediaTypeService = mediaTypeService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _contentAppDefinitions = contentAppDefinitions;
            _umbracoContextAccessor = umbracoContextAccessor;

            _tabsAndPropertiesMapper = new TabsAndPropertiesMapper<IMedia>(localizedTextService);
        }

        public void SetMaps(Mapper mapper)
        {
            mapper.Define<IMedia, MediaItemDisplay>((source, context) => new MediaItemDisplay(), Map);
            mapper.Define<IMedia, ContentItemBasic<ContentPropertyBasic>>((source, context) => new ContentItemBasic<ContentPropertyBasic>(), Map);
        }

        // Umbraco.Code.MapAll -Properties -Errors -Edited -Updater -Alias -IsContainer
        private void Map(IMedia source, MediaItemDisplay target, MapperContext context)
        {
            target.ContentApps = _contentAppDefinitions.GetContentAppsFor(source);
            target.ContentType = GetContentType(source, context.Mapper);
            target.ContentTypeAlias = source.ContentType.Alias;
            target.ContentTypeName = source.ContentType.Name;
            target.CreateDate = source.CreateDate;
            target.Icon = source.ContentType.Icon;
            target.Id = source.Id;
            target.IsChildOfListView = DermineIsChildOfListView(source);
            target.Key = source.Key;
            target.MediaLink = string.Join(",", source.GetUrls(Current.Configs.Settings().Content, _logger));
            target.Name = source.Name;
            target.Owner = GetOwner(source, context.Mapper);
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.SortOrder = source.SortOrder;
            target.State = null;
            target.Tabs = _tabsAndPropertiesMapper.Map(source, context);
            target.Trashed = source.Trashed;
            target.TreeNodeUrl = GetTreeNodeUrl<MediaTreeController>(source);
            target.Udi = Udi.Create(Constants.UdiEntityType.Media, source.Key);
            target.UpdateDate = source.UpdateDate;
            target.VariesByCulture = source.ContentType.VariesByCulture();
        }

        // Umbraco.Code.MapAll -Edited -Updater -Alias
        private void Map(IMedia source, ContentItemBasic<ContentPropertyBasic> target, MapperContext context)
        {
            target.ContentTypeAlias = source.ContentType.Alias;
            target.CreateDate = source.CreateDate;
            target.Icon = source.ContentType.Icon;
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.Owner = GetOwner(source, context.Mapper);
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.Properties = context.Mapper.Map<IEnumerable<ContentPropertyBasic>>(source.Properties);
            target.SortOrder = source.SortOrder;
            target.State = null;
            target.Trashed = source.Trashed;
            target.Udi = Udi.Create(Constants.UdiEntityType.Media, source.Key);
            target.UpdateDate = source.UpdateDate;
            target.VariesByCulture = source.ContentType.VariesByCulture();
        }

        private UserProfile GetOwner(IContentBase source, Mapper mapper)
        {
            var profile = source.GetCreatorProfile(_userService);
            return profile == null ? null : mapper.Map<IProfile, UserProfile>(profile);
        }

        private bool DermineIsChildOfListView(IMedia source)
        {
            // map the IsChildOfListView (this is actually if it is a descendant of a list view!)
            var parent = _mediaService.GetParent(source);
            return parent != null && (parent.ContentType.IsContainer || _mediaTypeService.HasContainerInPath(parent.Path));
        }

        private ContentTypeBasic GetContentType(IContentBase source, Mapper mapper)
        {
            // TODO: We can resolve the UmbracoContext from the IValueResolver options!
            // OMG
            if (HttpContext.Current != null && Composing.Current.UmbracoContext != null && Composing.Current.UmbracoContext.Security.CurrentUser != null
                && Composing.Current.UmbracoContext.Security.CurrentUser.AllowedSections.Any(x => x.Equals(Constants.Applications.Settings)))
            {
                var contentType = _contentTypeBaseServiceProvider.GetContentTypeOf(source);
                var contentTypeBasic = mapper.Map<IContentTypeComposition, ContentTypeBasic>(contentType);

                return contentTypeBasic;
            }
            //no access
            return null;
        }

        private string GetTreeNodeUrl<TController>(IContentBase source)
            where TController : ContentTreeControllerBase
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (umbracoContext == null) return null;

            var urlHelper = new UrlHelper(umbracoContext.HttpContext.Request.RequestContext);
            return urlHelper.GetUmbracoApiService<TController>(controller => controller.GetTreeNode(source.Key.ToString("N"), null));
        }
    }
}
