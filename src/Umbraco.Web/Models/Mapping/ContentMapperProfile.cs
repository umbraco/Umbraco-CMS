using System;
using System.Collections.Generic;
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
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            ILocalizationService localizationService)
        {
            // create, capture, cache
            var contentOwnerResolver = new OwnerResolver<IContent>(userService);
            var creatorResolver = new CreatorResolver(userService);
            var actionButtonsResolver = new ActionButtonsResolver(userService, contentService);
            var childOfListViewResolver = new ContentChildOfListViewResolver(contentService, contentTypeService);
            var contentTypeBasicResolver = new ContentTypeBasicResolver<IContent, ContentItemDisplay>(contentTypeBaseServiceProvider);
            var allowedTemplatesResolver = new AllowedTemplatesResolver(contentTypeService);
            var defaultTemplateResolver = new DefaultTemplateResolver();
            var variantResolver = new ContentVariantResolver(localizationService);
            var schedPublishReleaseDateResolver = new ScheduledPublishDateResolver(ContentScheduleAction.Release);
            var schedPublishExpireDateResolver = new ScheduledPublishDateResolver(ContentScheduleAction.Expire);

            //FROM IContent TO ContentItemDisplay
            CreateMap<IContent, ContentItemDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.MapFrom(src => creatorResolver.Resolve(src)))
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(variantResolver))
                .ForMember(dest => dest.ContentApps, opt => opt.MapFrom(contentAppResolver))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.ContentTypeName, opt => opt.MapFrom(src => src.ContentType.Name))
                .ForMember(dest => dest.IsContainer, opt => opt.MapFrom(src => src.ContentType.IsContainer))
                .ForMember(dest => dest.IsElement, opt => opt.MapFrom(src => src.ContentType.IsElement))
                .ForMember(dest => dest.IsBlueprint, opt => opt.MapFrom(src => src.Blueprint))
                .ForMember(dest => dest.IsChildOfListView, opt => opt.MapFrom(childOfListViewResolver))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.TemplateAlias, opt => opt.MapFrom(defaultTemplateResolver))
                .ForMember(dest => dest.Urls, opt => opt.MapFrom(contentUrlResolver))
                .ForMember(dest => dest.AllowPreview, opt => opt.Ignore())
                .ForMember(dest => dest.TreeNodeUrl, opt => opt.MapFrom(contentTreeNodeUrlResolver))
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Errors, opt => opt.Ignore())
                .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(contentTypeBasicResolver))
                .ForMember(dest => dest.AllowedTemplates, opt => opt.MapFrom(allowedTemplatesResolver))
                .ForMember(dest => dest.AllowedActions, opt => opt.MapFrom(src => actionButtonsResolver.Resolve(src)))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<IContent, ContentVariantDisplay>()
                .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.PublishDate))
                .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(schedPublishReleaseDateResolver))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(schedPublishExpireDateResolver))
                .ForMember(dest => dest.Segment, opt => opt.Ignore())
                .ForMember(dest => dest.Language, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.State, opt => opt.MapFrom<ContentSavedStateResolver<ContentPropertyDisplay>>())
                .ForMember(dest => dest.Tabs, opt => opt.MapFrom(tabsAndPropertiesResolver));

            //FROM IContent TO ContentItemBasic<ContentPropertyBasic, IContent>
            CreateMap<IContent, ContentItemBasic<ContentPropertyBasic>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src =>
                    Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.MapFrom(src => creatorResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom<UpdateDateResolver>())
                .ForMember(dest => dest.Name, opt => opt.MapFrom<NameResolver>())
                .ForMember(dest => dest.State, opt => opt.MapFrom<ContentBasicSavedStateResolver<ContentPropertyBasic>>())
                .ForMember(dest => dest.VariesByCulture, opt => opt.MapFrom(src => src.ContentType.VariesByCulture()));

            //FROM IContent TO ContentPropertyCollectionDto
            //NOTE: the property mapping for cultures relies on a culture being set in the mapping context
            CreateMap<IContent, ContentPropertyCollectionDto>();
        }

        /// <summary>
        /// Resolves the update date for a content item/content variant
        /// </summary>
        private class UpdateDateResolver : IValueResolver<IContent, ContentItemBasic<ContentPropertyBasic>, DateTime>
        {
            public DateTime Resolve(IContent source, ContentItemBasic<ContentPropertyBasic> destination, DateTime destMember, ResolutionContext context)
            {
                // invariant = global date
                if (!source.ContentType.VariesByCulture()) return source.UpdateDate;

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
            public string Resolve(IContent source, ContentItemBasic<ContentPropertyBasic> destination, string destMember, ResolutionContext context)
            {
                // invariant = only 1 name
                if (!source.ContentType.VariesByCulture()) return source.Name;

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


        private class AllowedTemplatesResolver : IValueResolver<IContent, ContentItemDisplay, IDictionary<string, string>>
        {
            private readonly IContentTypeService _contentTypeService;

            public AllowedTemplatesResolver(IContentTypeService contentTypeService)
            {
                _contentTypeService = contentTypeService;
            }

            public IDictionary<string, string> Resolve(IContent source, ContentItemDisplay destination, IDictionary<string, string> destMember, ResolutionContext context)
            {
                var contentType = _contentTypeService.Get(source.ContentTypeId);

                return contentType.AllowedTemplates
                    .Where(t => t.Alias.IsNullOrWhiteSpace() == false && t.Name.IsNullOrWhiteSpace() == false)
                    .ToDictionary(t => t.Alias, t => t.Name);
            }
        }
    }

}
