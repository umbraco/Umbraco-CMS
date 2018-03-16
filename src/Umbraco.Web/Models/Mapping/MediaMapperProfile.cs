using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for media.
    /// </summary>
    internal class MediaMapperProfile : Profile
    {
        public MediaMapperProfile(IUserService userService, ILocalizedTextService textService, IDataTypeService dataTypeService, IMediaService mediaService, ILogger logger)
        {
            // create, capture, cache
            var mediaOwnerResolver = new OwnerResolver<IMedia>(userService);
            var tabsAndPropertiesResolver = new TabsAndPropertiesResolver(textService);

            //FROM IMedia TO MediaItemDisplay
            CreateMap<IMedia, MediaItemDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(content => Udi.Create(Constants.UdiEntityType.Media, content.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => mediaOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(content => content.ContentType.Icon))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(content => content.ContentType.Alias))
                .ForMember(dest => dest.IsChildOfListView, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(content => content.Trashed))
                .ForMember(dest => dest.ContentTypeName, opt => opt.MapFrom(content => content.ContentType.Name))
                .ForMember(dest => dest.Properties, opt => opt.Ignore())
                .ForMember(dest => dest.TreeNodeUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Errors, opt => opt.Ignore())
                .ForMember(dest => dest.Published, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.IsContainer, opt => opt.Ignore())
                .ForMember(dest => dest.Tabs, opt => opt.ResolveUsing(src => tabsAndPropertiesResolver.Resolve(src)))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .AfterMap((src, dest) => AfterMap(src, dest, dataTypeService, textService, logger, mediaService));

            //FROM IMedia TO ContentItemBasic<ContentPropertyBasic, IMedia>
            CreateMap<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.Media, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => mediaOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.Published, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            //FROM IMedia TO ContentItemDto<IMedia>
            CreateMap<IMedia, ContentItemDto<IMedia>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.Media, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => mediaOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Published, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());
        }

        private static void AfterMap(IMedia media, MediaItemDisplay display, IDataTypeService dataTypeService, ILocalizedTextService localizedText, ILogger logger, IMediaService mediaService)
        {
            // Adapted from ContentModelMapper
            //map the IsChildOfListView (this is actually if it is a descendant of a list view!)
            var parent = media.Parent();
            display.IsChildOfListView = parent != null && (parent.ContentType.IsContainer || Current.Services.ContentTypeService.HasContainerInPath(parent.Path));

            //map the tree node url
            if (HttpContext.Current != null)
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var url = urlHelper.GetUmbracoApiService<MediaTreeController>(controller => controller.GetTreeNode(display.Id.ToString(), null));
                display.TreeNodeUrl = url;
            }

            if (media.ContentType.IsContainer)
            {
                TabsAndPropertiesResolver.AddListView(display, "media", dataTypeService, localizedText);
            }

            var genericProperties = new List<ContentPropertyDisplay>
            {
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("content/mediatype"),
                    Value = localizedText.UmbracoDictionaryTranslate(display.ContentTypeName),
                    View = Current.PropertyEditors[Constants.PropertyEditors.Aliases.NoEdit].GetValueEditor().View
                }
            };

            TabsAndPropertiesResolver.MapGenericProperties(media, display, localizedText, genericProperties, properties =>
            {
                if (HttpContext.Current != null && UmbracoContext.Current != null && UmbracoContext.Current.Security.CurrentUser != null
                    && UmbracoContext.Current.Security.CurrentUser.AllowedSections.Any(x => x.Equals(Constants.Applications.Settings)))
                {
                    var mediaTypeLink = string.Format("#/settings/mediatypes/edit/{0}", media.ContentTypeId);

                    //Replace the doctype property
                    var docTypeProperty = properties.First(x => x.Alias == string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                    docTypeProperty.Value = new List<object>
                    {
                        new
                        {
                            linkText = media.ContentType.Name,
                            url = mediaTypeLink,
                            target = "_self",
                            icon = "icon-item-arrangement"
                        }
                    };
                    docTypeProperty.View = "urllist";
                }

                // inject 'Link to media' as the first generic property
                var links = media.GetUrls(UmbracoConfig.For.UmbracoSettings().Content, logger);
                if (links.Any())
                {
                    var link = new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}urls", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = localizedText.Localize("media/urls"),
                        Value = string.Join(",", links),
                        View = "urllist"
                    };
                    properties.Insert(0, link);
                }
            });
        }
    }
}
