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
                  .ForMember(
                      dto => dto.IsChildOfListView,
                      expression => expression.MapFrom(content => content.Parent().ContentType.IsContainer))
                  .ForMember(
                      dto => dto.ContentTypeName,
                      expression => expression.MapFrom(content => content.ContentType.Name))
                  .ForMember(display => display.Properties, expression => expression.Ignore())
                  .ForMember(display => display.Tabs, expression => expression.ResolveUsing<TabsAndPropertiesResolver>())
                  .AfterMap(AfterMap);

            //FROM IMedia TO ContentItemBasic<ContentPropertyBasic, IMedia>
            config.CreateMap<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>()
                  .ForMember(
                      dto => dto.Owner,
                      expression => expression.ResolveUsing<OwnerResolver<IMedia>>())
                  .ForMember(
                      dto => dto.Icon,
                      expression => expression.MapFrom(content => content.ContentType.Icon))
                  .ForMember(
                      dto => dto.ContentTypeAlias,
                      expression => expression.MapFrom(content => content.ContentType.Alias));

            //FROM IMedia TO ContentItemDto<IMedia>
            config.CreateMap<IMedia, ContentItemDto<IMedia>>()
                  .ForMember(
                      dto => dto.Owner,
                      expression => expression.ResolveUsing<OwnerResolver<IMedia>>());
        }

        private static void AfterMap(IMedia media, MediaItemDisplay display)
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
                TabsAndPropertiesResolver.AddContainerView(display, "media");
            }

            TabsAndPropertiesResolver.MapGenericProperties(media, display);
        }

    }
}
