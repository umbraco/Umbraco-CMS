using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using Umbraco.Core;
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
                .ForMember(
                    dto => dto.Owner,
                    expression => expression.ResolveUsing<OwnerResolver<IMedia>>())
                .ForMember(
                    dto => dto.Icon,
                    expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(
                    dto => dto.ContentTypeAlias,
                    expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(display => display.IsChildOfListView, expression => expression.Ignore())
                .ForMember(
                    dto => dto.Trashed,
                    expression => expression.MapFrom(content => content.Trashed))
                .ForMember(
                    dto => dto.ContentTypeName,
                    expression => expression.MapFrom(content => content.ContentType.Name))
                .ForMember(display => display.Properties, expression => expression.Ignore())
                .ForMember(display => display.TreeNodeUrl, expression => expression.Ignore())
                .ForMember(display => display.Notifications, expression => expression.Ignore())
                .ForMember(display => display.Errors, expression => expression.Ignore())
                .ForMember(display => display.Published, expression => expression.Ignore())
                .ForMember(display => display.Updater, expression => expression.Ignore())
                .ForMember(display => display.Alias, expression => expression.Ignore())
                .ForMember(display => display.IsContainer, expression => expression.Ignore())
                .ForMember(display => display.Tabs, expression => expression.ResolveUsing<TabsAndPropertiesResolver>())
                .AfterMap((media, display) => AfterMap(media, display, applicationContext.Services.DataTypeService));

            //FROM IMedia TO ContentItemBasic<ContentPropertyBasic, IMedia>
            config.CreateMap<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>()
                .ForMember(
                    dto => dto.Owner,
                    expression => expression.ResolveUsing<OwnerResolver<IMedia>>())
                .ForMember(
                    dto => dto.Icon,
                    expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(
                    dto => dto.Trashed,
                    expression => expression.MapFrom(content => content.Trashed))
                .ForMember(
                    dto => dto.ContentTypeAlias,
                    expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(x => x.Published, expression => expression.Ignore())
                .ForMember(x => x.Updater, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore());

            //FROM IMedia TO ContentItemDto<IMedia>
            config.CreateMap<IMedia, ContentItemDto<IMedia>>()
                .ForMember(
                    dto => dto.Owner,
                    expression => expression.ResolveUsing<OwnerResolver<IMedia>>())
                .ForMember(x => x.Published, expression => expression.Ignore())
                .ForMember(x => x.Updater, expression => expression.Ignore())
                .ForMember(x => x.Icon, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore());
        }

        private static void AfterMap(IMedia media, MediaItemDisplay display, IDataTypeService dataTypeService)
        {
            //map the tree node url
            if (HttpContext.Current != null)
            {
                var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));
                var url = urlHelper.GetUmbracoApiService<MediaTreeController>(controller => controller.GetTreeNode(display.Id.ToString(), null));
                display.TreeNodeUrl = url;
            }
            
            if (media.ContentType.IsContainer)
            {
                TabsAndPropertiesResolver.AddListView(display, "media", dataTypeService);
            }

            TabsAndPropertiesResolver.MapGenericProperties(media, display);
        }

    }
}
