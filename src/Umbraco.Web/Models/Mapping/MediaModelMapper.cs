using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for media.
    /// </summary>
    internal class MediaModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            //FROM IMedia TO MediaItemDisplay
            config.CreateMap<IMedia, MediaItemDisplay>()
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.Media, content.Key)))
                .ForMember(display => display.Owner, expression => expression.ResolveUsing(new OwnerResolver<IMedia>()))
                .ForMember(display => display.Icon, expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(display => display.ContentTypeAlias, expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(display => display.IsChildOfListView, expression => expression.ResolveUsing(new ChildOfListViewResolver(applicationContext.Services.MediaService, applicationContext.Services.ContentTypeService)))
                .ForMember(display => display.Trashed, expression => expression.MapFrom(content => content.Trashed))
                .ForMember(display => display.ContentTypeName, expression => expression.MapFrom(content => content.ContentType.Name))
                .ForMember(display => display.Properties, expression => expression.Ignore())
                .ForMember(display => display.TreeNodeUrl, opt => opt.ResolveUsing(new ContentTreeNodeUrlResolver<IMedia, MediaTreeController>()))
                .ForMember(display => display.Notifications, expression => expression.Ignore())
                .ForMember(display => display.Errors, expression => expression.Ignore())
                .ForMember(display => display.Published, expression => expression.Ignore())
                .ForMember(display => display.Updater, expression => expression.Ignore())
                .ForMember(display => display.Alias, expression => expression.Ignore())
                .ForMember(display => display.IsContainer, expression => expression.Ignore())
                .ForMember(display => display.HasPublishedVersion, expression => expression.Ignore())
                .ForMember(display => display.Tabs, expression => expression.ResolveUsing(new TabsAndPropertiesResolver<IMedia>(applicationContext.Services.TextService)))
                .ForMember(display => display.ContentType, expression => expression.ResolveUsing<MediaTypeBasicResolver>())
                .ForMember(display => display.MediaLink, expression => expression.ResolveUsing(
                    content => string.Join(",", content.GetUrls(UmbracoConfig.For.UmbracoSettings().Content, applicationContext.ProfilingLogger.Logger))))
                .AfterMap((media, display) =>
                {
                    if (media.ContentType.IsContainer)
                    {
                        TabsAndPropertiesResolver<IMedia>.AddListView(display, "media", applicationContext.Services.DataTypeService, applicationContext.Services.TextService);
                    }
                });

            //FROM IMedia TO ContentItemBasic<ContentPropertyBasic, IMedia>
            config.CreateMap<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>()
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.Media, content.Key)))
                .ForMember(dto => dto.Owner, expression => expression.ResolveUsing(new OwnerResolver<IMedia>()))
                .ForMember(dto => dto.Icon, expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(dto => dto.Trashed, expression => expression.MapFrom(content => content.Trashed))
                .ForMember(dto => dto.ContentTypeAlias, expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(dto => dto.Published, expression => expression.Ignore())
                .ForMember(dto => dto.Updater, expression => expression.Ignore())
                .ForMember(dto => dto.Alias, expression => expression.Ignore())
                .ForMember(dto => dto.HasPublishedVersion, expression => expression.Ignore());

            //FROM IMedia TO ContentItemDto<IMedia>
            config.CreateMap<IMedia, ContentItemDto<IMedia>>()
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.Media, content.Key)))
                .ForMember(dto => dto.Owner, expression => expression.ResolveUsing(new OwnerResolver<IMedia>()))
                .ForMember(dto => dto.Published, expression => expression.Ignore())
                .ForMember(dto => dto.Updater, expression => expression.Ignore())
                .ForMember(dto => dto.Icon, expression => expression.Ignore())
                .ForMember(dto => dto.Alias, expression => expression.Ignore())
                .ForMember(dto => dto.HasPublishedVersion, expression => expression.Ignore());            
        }        

        private class ChildOfListViewResolver : ValueResolver<IMedia, bool>
        {
            private readonly IMediaService _mediaService;
            private readonly IContentTypeService _contentTypeService;

            public ChildOfListViewResolver(IMediaService mediaService, IContentTypeService contentTypeService)
            {
                _mediaService = mediaService;
                _contentTypeService = contentTypeService;
            }

            protected override bool ResolveCore(IMedia source)
            {
                // map the IsChildOfListView (this is actually if it is a descendant of a list view!)
                var parent = _mediaService.GetParent(source);
                return parent != null && (parent.ContentType.IsContainer || _contentTypeService.HasContainerInPath(parent.Path));
            }
        }


        /// <summary>
        /// Resolves a <see cref="ContentTypeBasic"/> from the <see cref="IContent"/> item and checks if the current user
        /// has access to see this data
        /// </summary>
        private class MediaTypeBasicResolver : ValueResolver<IMedia, ContentTypeBasic>
        {
            protected override ContentTypeBasic ResolveCore(IMedia source)
            {
                //TODO: We can resolve the UmbracoContext from the IValueResolver options!
                if (HttpContext.Current != null && UmbracoContext.Current != null &&
                    UmbracoContext.Current.Security.CurrentUser != null
                    && UmbracoContext.Current.Security.CurrentUser.AllowedSections.Any(x => x.Equals(Constants
                        .Applications.Settings)))
                {
                    var contentTypeBasic = Mapper.Map<ContentTypeBasic>(source.ContentType);
                    return contentTypeBasic;
                }
                //no access
                return null;
            }

        }
    }
}
