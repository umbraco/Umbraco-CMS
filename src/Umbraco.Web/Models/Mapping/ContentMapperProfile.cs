using System;
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
        public ContentMapperProfile(
            ContentUrlResolver contentUrlResolver,
            ContentTreeNodeUrlResolver<IContent, ContentTreeController> contentTreeNodeUrlResolver,
            TabsAndPropertiesResolver<IContent, ContentVariantDisplay> tabsAndPropertiesResolver,
            ContentAppResolver contentAppResolver,
            IUserService userService,
            IContentService contentService,
            IContentTypeService contentTypeService,
            ILocalizationService localizationService)
        {
            // create, capture, cache
            var contentOwnerResolver = new OwnerResolver<IContent>(userService);
            var creatorResolver = new CreatorResolver(userService);
            var actionButtonsResolver = new ActionButtonsResolver(userService, contentService);
            var childOfListViewResolver = new ContentChildOfListViewResolver(contentService, contentTypeService);
            var contentTypeBasicResolver = new ContentTypeBasicResolver<IContent, ContentItemDisplay>();
            var defaultTemplateResolver = new DefaultTemplateResolver();
            var variantResolver = new ContentVariantResolver(localizationService);
            var contentSavedStateResolver = new ContentSavedStateResolver();
            
            //FROM IContent TO ContentItemDisplay
            CreateMap<IContent, ContentItemDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.ResolveUsing(src => creatorResolver.Resolve(src)))
                .ForMember(dest => dest.Variants, opt => opt.ResolveUsing(variantResolver))
                .ForMember(dest => dest.ContentApps, opt => opt.ResolveUsing(contentAppResolver))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.ContentTypeName, opt => opt.MapFrom(src => src.ContentType.Name))
                .ForMember(dest => dest.IsContainer, opt => opt.MapFrom(src => src.ContentType.IsContainer))
                .ForMember(dest => dest.IsBlueprint, opt => opt.MapFrom(src => src.Blueprint))
                .ForMember(dest => dest.IsChildOfListView, opt => opt.ResolveUsing(childOfListViewResolver))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.TemplateAlias, opt => opt.ResolveUsing(defaultTemplateResolver))
                .ForMember(dest => dest.Urls, opt => opt.ResolveUsing(contentUrlResolver))
                .ForMember(dest => dest.AllowPreview, opt => opt.Ignore())
                .ForMember(dest => dest.TreeNodeUrl, opt => opt.ResolveUsing(contentTreeNodeUrlResolver))
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Errors, opt => opt.Ignore())
                .ForMember(dest => dest.DocumentType, opt => opt.ResolveUsing(contentTypeBasicResolver))
                .ForMember(dest => dest.AllowedTemplates, opt =>
                    opt.MapFrom(content => content.ContentType.AllowedTemplates
                        .Where(t => t.Alias.IsNullOrWhiteSpace() == false && t.Name.IsNullOrWhiteSpace() == false)
                        .ToDictionary(t => t.Alias, t => t.Name)))                
                .ForMember(dest => dest.AllowedActions, opt => opt.ResolveUsing(src => actionButtonsResolver.Resolve(src)))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<IContent, ContentVariantDisplay>()
                .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.PublishDate))
                .ForMember(dest => dest.Segment, opt => opt.Ignore())
                .ForMember(dest => dest.Language, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.State, opt => opt.ResolveUsing(contentSavedStateResolver))
                .ForMember(dest => dest.Tabs, opt => opt.ResolveUsing(tabsAndPropertiesResolver));

            //FROM IContent TO ContentItemBasic<ContentPropertyBasic, IContent>
            CreateMap<IContent, ContentItemBasic<ContentPropertyBasic>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src =>
                    Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.ResolveUsing(src => creatorResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing<CultureUpdateDateResolver>())
                .ForMember(dest => dest.Published, opt => opt.ResolveUsing<CulturePublishedResolver>())
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing<CultureNameResolver>())
                .ForMember(dest => dest.State, opt => opt.ResolveUsing<CultureStateResolver>());

            //FROM IContent TO ContentPropertyCollectionDto
            //NOTE: the property mapping for cultures relies on a culture being set in the mapping context
            CreateMap<IContent, ContentPropertyCollectionDto>();
        }
    }

    internal class CultureStateResolver : IValueResolver<IContent, ContentItemBasic<ContentPropertyBasic>, ContentSavedState?>
    {
        //WB: Note this is same logic as ContentSavedStateResolver.cs
        //But this is for ContentItemBasic instead of ContentVariantDisplay
        public ContentSavedState? Resolve(IContent source, ContentItemBasic<ContentPropertyBasic> destination, ContentSavedState? destMember, ResolutionContext context)
        {
            PublishedState publishedState;
            bool isEdited;
            

            if (source.ContentType.VariesByCulture())
            {
                //Get the culture from the context which will be set during the mapping operation for each variant
                var culture = context.GetCulture();

                //a culture needs to be in the context for a variant content item
                if (culture == null)
                    throw new InvalidOperationException($"No culture found in mapping operation when one is required for a culture variant");

                publishedState = source.PublishedState == PublishedState.Unpublished //if the entire document is unpublished, then flag every variant as unpublished
                    ? PublishedState.Unpublished
                    : source.IsCulturePublished(culture)
                        ? PublishedState.Published
                        : PublishedState.Unpublished;

                isEdited = source.IsCultureEdited(culture);
            }
            else
            {
                publishedState = source.PublishedState == PublishedState.Unpublished
                    ? PublishedState.Unpublished
                    : PublishedState.Published;

                isEdited = source.Edited;
            }

            if (publishedState == PublishedState.Unpublished)
                return isEdited && source.Id > 0 ? ContentSavedState.Draft : ContentSavedState.NotCreated;

            if (publishedState == PublishedState.Published)
                return isEdited ? ContentSavedState.PublishedPendingChanges : ContentSavedState.Published;

            throw new NotSupportedException($"PublishedState {publishedState} is not supported.");
        }
    }

    internal class CultureUpdateDateResolver : IValueResolver<IContent, ContentItemBasic<ContentPropertyBasic>, DateTime>
    {
        public DateTime Resolve(IContent source, ContentItemBasic<ContentPropertyBasic> destination, DateTime destMember, ResolutionContext context)
        {
            var culture = context.GetCulture();

            //a culture needs to be in the context for a variant content item
            if (culture == null || source.ContentType.VariesByCulture() == false)
                return source.UpdateDate;

            var pubDate = source.GetPublishDate(culture);
            return pubDate.HasValue ? pubDate.Value : source.UpdateDate;
        }
    }

    internal class CulturePublishedResolver : IValueResolver<IContent, ContentItemBasic<ContentPropertyBasic>, bool>
    {
        public bool Resolve(IContent source, ContentItemBasic<ContentPropertyBasic> destination, bool destMember, ResolutionContext context)
        {
            var culture = context.GetCulture();

            //a culture needs to be in the context for a variant content item
            if (culture == null || source.ContentType.VariesByCulture() == false)
                return source.Published;

            return source.IsCulturePublished(culture);
        }
    }

    internal class CultureNameResolver : IValueResolver<IContent, ContentItemBasic<ContentPropertyBasic>, string>
    {
        public string Resolve(IContent source, ContentItemBasic<ContentPropertyBasic> destination, string destMember, ResolutionContext context)
        {
            var culture = context.GetCulture();

            //a culture needs to be in the context for a variant content item
            if (culture == null || source.ContentType.VariesByCulture() == false)
                return source.Name;
            
            if (source.CultureNames.TryGetValue(culture, out var name) && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            else
            {
                return $"({ source.Name })";
            }
        }
    }
}
