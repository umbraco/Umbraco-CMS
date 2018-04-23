using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares how model mappings for content
    /// </summary>
    internal class ContentMapperProfile : Profile
    {
        public ContentMapperProfile(IUserService userService, ILocalizedTextService textService, IContentService contentService, IContentTypeService contentTypeService, IDataTypeService dataTypeService, ILocalizationService localizationService)
        {
            // create, capture, cache
            var contentOwnerResolver = new OwnerResolver<IContent>(userService);
            var creatorResolver = new CreatorResolver(userService);
            var actionButtonsResolver = new ActionButtonsResolver(userService, contentService);
            var tabsAndPropertiesResolver = new TabsAndPropertiesResolver<IContent, ContentItemDisplay>(textService);
            var childOfListViewResolver = new ContentChildOfListViewResolver(contentService, contentTypeService);
            var contentTypeBasicResolver = new ContentTypeBasicResolver<IContent, ContentItemDisplay>();
            var contentTreeNodeUrlResolver = new ContentTreeNodeUrlResolver<IContent, ContentTreeController>();
            var defaultTemplateResolver = new DefaultTemplateResolver();
            var contentUrlResolver = new ContentUrlResolver();
            var variantResolver = new ContentItemDisplayVariationResolver(localizationService);

            //FROM IContent TO ContentItemDisplay
            CreateMap<IContent, ContentItemDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.ResolveUsing(src => creatorResolver.Resolve(src)))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing<ContentItemDisplayNameResolver>())
                .ForMember(dest => dest.Variants, opt => opt.ResolveUsing(variantResolver))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.ContentTypeName, opt => opt.MapFrom(src => src.ContentType.Name))
                .ForMember(dest => dest.IsContainer, opt => opt.MapFrom(src => src.ContentType.IsContainer))
                .ForMember(dest => dest.IsBlueprint, opt => opt.MapFrom(src => src.Blueprint))
                .ForMember(dest => dest.IsChildOfListView, opt => opt.ResolveUsing(childOfListViewResolver))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.PublishDate))
                .ForMember(dest => dest.TemplateAlias, opt => opt.ResolveUsing(defaultTemplateResolver))
                .ForMember(dest => dest.Urls, opt => opt.ResolveUsing(contentUrlResolver))
                .ForMember(dest => dest.Properties, opt => opt.Ignore())
                .ForMember(dest => dest.AllowPreview, opt => opt.Ignore())
                .ForMember(dest => dest.TreeNodeUrl, opt => opt.ResolveUsing(contentTreeNodeUrlResolver))
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Errors, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.DocumentType, opt => opt.ResolveUsing(contentTypeBasicResolver))
                .ForMember(dest => dest.AllowedTemplates, opt =>
                    opt.MapFrom(content => content.ContentType.AllowedTemplates
                        .Where(t => t.Alias.IsNullOrWhiteSpace() == false && t.Name.IsNullOrWhiteSpace() == false)
                        .ToDictionary(t => t.Alias, t => t.Name)))
                .ForMember(dest => dest.Tabs, opt => opt.ResolveUsing(tabsAndPropertiesResolver))
                .ForMember(dest => dest.AllowedActions, opt => opt.ResolveUsing(src => actionButtonsResolver.Resolve(src)))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .AfterMap((content, display) =>
                {
                    if (content.ContentType.IsContainer)
                        TabsAndPropertiesResolver.AddListView(display, "content", dataTypeService, textService);
                });

            //FROM IContent TO ContentItemBasic<ContentPropertyBasic, IContent>
            CreateMap<IContent, ContentItemBasic<ContentPropertyBasic, IContent>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src =>
                    Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.ResolveUsing(src => creatorResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            //FROM IContent TO ContentItemDto<IContent>
            CreateMap<IContent, ContentItemDto<IContent>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src =>
                    Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());
        }
    }
}
