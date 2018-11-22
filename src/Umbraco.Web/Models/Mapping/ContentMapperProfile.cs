using System;
using System.Linq;
using AutoMapper;
using Lucene.Net.Search;
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
            var contentTypeBasicResolver = new ContentTypeBasicResolver<IContent, ContentItemDisplay>(contentTypeService);
            var defaultTemplateResolver = new DefaultTemplateResolver(contentTypeService);
            var variantResolver = new ContentVariantResolver(localizationService, contentTypeService);
            var updateDateResolver = new UpdateDateResolver(contentTypeService);
            var nameResolver = new NameResolver(contentTypeService);
            var contentSavedStateResolver = new ContentSavedStateResolver<ContentPropertyDisplay>(contentTypeService);
            var contentBasicSavedStateResolver = new ContentBasicSavedStateResolver<ContentPropertyBasic>(contentTypeService);

            var schedPublishReleaseDateResolver = new ScheduledPublishDateResolver(ContentScheduleAction.Release);
            var schedPublishExpireDateResolver = new ScheduledPublishDateResolver(ContentScheduleAction.Expire);

            //FROM IContent TO ContentItemDisplay
            CreateMap<IContent, ContentItemDisplay>()
                .ConstructUsing((content, context)=>GetInitialContentItemDisplay(content, context, contentTypeService))
                .ForMember(dest => dest.Icon, opt => opt.Ignore()) // Handled in ConstructUsing
                .ForMember(dest => dest.ContentTypeName, opt => opt.Ignore()) // Handled in ConstructUsing
                .ForMember(dest => dest.IsContainer, opt => opt.Ignore()) // Handled in ConstructUsing
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.Ignore()) // Handled in ConstructUsing
                .ForMember(dest => dest.AllowedTemplates, opt => opt.Ignore()) // Handled in ConstructUsing
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, src.Key)))

                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.ResolveUsing(src => creatorResolver.Resolve(src)))
                .ForMember(dest => dest.Variants, opt => opt.ResolveUsing(variantResolver))
                .ForMember(dest => dest.ContentApps, opt => opt.ResolveUsing(contentAppResolver))
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
                .ForMember(dest => dest.AllowedActions, opt => opt.ResolveUsing(src => actionButtonsResolver.Resolve(src)))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<IContent, ContentVariantDisplay>()
                .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.PublishDate))
                .ForMember(dest => dest.ReleaseDate, opt => opt.ResolveUsing(schedPublishReleaseDateResolver))
                .ForMember(dest => dest.ExpireDate, opt => opt.ResolveUsing(schedPublishExpireDateResolver))
                .ForMember(dest => dest.Segment, opt => opt.Ignore())
                .ForMember(dest => dest.Language, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.State, opt => opt.ResolveUsing(contentSavedStateResolver))
                .ForMember(dest => dest.Tabs, opt => opt.ResolveUsing(tabsAndPropertiesResolver));

            //FROM IContent TO ContentItemBasic<ContentPropertyBasic, IContent>
            CreateMap<IContent, ContentItemBasic<ContentPropertyBasic>>()
                .ConstructUsing((content, context)=>GetInitialContentItemBasic(content, context, contentTypeService))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.Ignore()) // Handled in ConstructUsing
                .ForMember(dest => dest.VariesByCulture, opt => opt.Ignore()) // Handled in ConstructUsing
                .ForMember(dest => dest.Icon, opt => opt.Ignore()) // Handled in ConstructUsing
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src =>
                    Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.ResolveUsing(src => creatorResolver.Resolve(src)))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing((src, dest, destMember, context) => updateDateResolver.Resolve(src, dest, destMember, context)))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing((src, dest, destMember, context) => nameResolver.Resolve(src, dest, destMember, context)))
                .ForMember(dest => dest.State, opt => opt.ResolveUsing((src, dest, destMember, context) => contentBasicSavedStateResolver.Resolve(src, dest, destMember, context)));

            //FROM IContent TO ContentPropertyCollectionDto
            //NOTE: the property mapping for cultures relies on a culture being set in the mapping context
            CreateMap<IContent, ContentPropertyCollectionDto>();
        }

        private ContentItemBasic<ContentPropertyBasic> GetInitialContentItemBasic(IContent src, ResolutionContext context, IContentTypeService contentTypeService)
        {
            var contentType = contentTypeService.Get(src.ContentTypeId);

            var result = new ContentItemBasic<ContentPropertyBasic>()
            {
                VariesByCulture = contentType.VariesByCulture(),
                Icon = contentType.Icon,
                ContentTypeAlias = contentType.Alias
            };

            return result;
        }

        private ContentItemDisplay GetInitialContentItemDisplay(IContent src, ResolutionContext context, IContentTypeService contentTypeService)
        {
            var contentType = contentTypeService.Get(src.ContentTypeId);

            var result = new ContentItemDisplay
            {
                Icon = contentType.Icon,
                ContentTypeAlias = contentType.Alias,
                ContentTypeName = contentType.Name,
                IsContainer = contentType.IsContainer,
                AllowedTemplates = contentType.AllowedTemplates
                    .Where(t => t.Alias.IsNullOrWhiteSpace() == false && t.Name.IsNullOrWhiteSpace() == false)
                    .ToDictionary(t => t.Alias, t => t.Name)
            };



            return result;
        }

        /// <summary>
        /// Resolves the update date for a content item/content variant
        /// </summary>
        private class UpdateDateResolver : IValueResolver<IContent, ContentItemBasic<ContentPropertyBasic>, DateTime>
        {
            private readonly IContentTypeService _contentTypeService;

            public UpdateDateResolver(IContentTypeService contentTypeService)
            {
                _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
            }

            public DateTime Resolve(IContent source, ContentItemBasic<ContentPropertyBasic> destination, DateTime destMember, ResolutionContext context)
            {

                var contentType = _contentTypeService.Get(source.ContentTypeId);
                // invariant = global date
                if (!contentType.VariesByCulture()) return source.UpdateDate;

                // variant = depends on culture
                var culture = context.Options.GetCulture();

                // if there's no culture here, the issue is somewhere else (UI, whatever) - throw!
                if (culture == null)
                    throw new InvalidOperationException("Missing culture in mapping options.");

                // if we don't have a date for a culture, it means the culture is not available, and
                // hey we should probably not be mapping it, but it's too late, return a fallback date
                var date = source.GetUpdateDate(culture);
                return date ?? source.UpdateDate;
            }
        }

        /// <summary>
        /// Resolves the name for a content item/content variant
        /// </summary>
        private class NameResolver : IValueResolver<IContent, ContentItemBasic<ContentPropertyBasic>, string>
        {
            private readonly IContentTypeService _contentTypeService;

            public NameResolver(IContentTypeService contentTypeService)
            {
                _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));

            }
            public string Resolve(IContent source, ContentItemBasic<ContentPropertyBasic> destination, string destMember, ResolutionContext context)
            {
                var contentType = _contentTypeService.Get(source.ContentTypeId);
                // invariant = only 1 name
                if (!contentType.VariesByCulture()) return source.Name;

                // variant = depends on culture
                var culture = context.Options.GetCulture();

                // if there's no culture here, the issue is somewhere else (UI, whatever) - throw!
                if (culture == null)
                    throw new InvalidOperationException("Missing culture in mapping options.");

                // if we don't have a name for a culture, it means the culture is not available, and
                // hey we should probably not be mapping it, but it's too late, return a fallback name
                return source.CultureInfos.TryGetValue(culture, out var name) && !name.Name.IsNullOrWhiteSpace() ? name.Name : $"(({source.Name}))";
            }
        }
    }
}
