using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <inheritdoc />
    /// <summary>
    /// The dictionary model mapper.
    /// </summary>
    internal class DictionaryMapperProfile : Profile
    {
        public DictionaryMapperProfile(ILocalizationService localizationService)
        {
            CreateMap<IDictionaryItem, EntityBasic>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(sheet => sheet.Id))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(sheet => sheet.ItemKey))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(sheet => sheet.Key))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(sheet => sheet.ItemKey))
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore());

            CreateMap<IDictionaryItem, DictionaryDisplay>()
                .ForMember(x => x.Translations, expression => expression.Ignore())
                .ForMember(x => x.Notifications, expression => expression.Ignore())
                .ForMember(x => x.Icon, expression => expression.Ignore())
                .ForMember(x => x.Trashed, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore())
                .ForMember(x => x.Path, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore())
                .ForMember(
                    x => x.Udi,
                    expression => expression.MapFrom(
                        content => Udi.Create(Constants.UdiEntityType.DictionaryItem, content.Key))).ForMember(
                    x => x.Name,
                    expression => expression.MapFrom(content => content.ItemKey))
                .AfterMap(
                    (src, dest) =>
                    {
                        // build up the path to make it possible to set active item in tree
                        // TODO: check if there is a better way
                        if (src.ParentId.HasValue)
                        {
                            var ids = new List<int> { -1 };


                            var parentIds = new List<int>();

                            this.GetParentId(src.ParentId.Value, localizationService, parentIds);

                            parentIds.Reverse();

                            ids.AddRange(parentIds);

                            ids.Add(src.Id);

                            dest.Path = string.Join(",", ids);
                        }
                        else
                        {
                            dest.Path = "-1," + src.Id;
                        }

                        // add all languages and  the translations
                        foreach (var lang in localizationService.GetAllLanguages())
                        {
                            var langId = lang.Id;
                            var translation = src.Translations.FirstOrDefault(x => x.LanguageId == langId);

                            dest.Translations.Add(new DictionaryTranslationDisplay
                            {
                                IsoCode = lang.IsoCode,
                                DisplayName = lang.CultureInfo.DisplayName,
                                Translation = (translation != null) ? translation.Value : string.Empty,
                                LanguageId = lang.Id
                            });
                        }
                    });

            CreateMap<IDictionaryItem, DictionaryOverviewDisplay>()
                .ForMember(dest => dest.Level, expression => expression.Ignore())
                .ForMember(dest => dest.Translations, expression => expression.Ignore())
                .ForMember(
                    x => x.Name,
                    expression => expression.MapFrom(content => content.ItemKey))
                .AfterMap(
                    (src, dest) =>
                    {
                        // add all languages and  the translations
                        foreach (var lang in localizationService.GetAllLanguages())
                        {
                            var langId = lang.Id;
                            var translation = src.Translations.FirstOrDefault(x => x.LanguageId == langId);

                            dest.Translations.Add(
                                new DictionaryOverviewTranslationDisplay
                                {
                                    DisplayName = lang.CultureInfo.DisplayName,
                                    HasTranslation = translation != null && string.IsNullOrEmpty(translation.Value) == false
                                });
                        }
                    });
        }

        /// <summary>
        /// Goes up the dictionary tree to get all parent ids
        /// </summary>
        /// <param name="parentId">
        /// The parent id.
        /// </param>
        /// <param name="localizationService">
        /// The localization service.
        /// </param>
        /// <param name="ids">
        /// The ids.
        /// </param>
        private void GetParentId(Guid parentId, ILocalizationService localizationService, List<int> ids)
        {
            var dictionary = localizationService.GetDictionaryItemById(parentId);

            if (dictionary == null)
                return;

            ids.Add(dictionary.Id);

            if (dictionary.ParentId.HasValue)
                GetParentId(dictionary.ParentId.Value, localizationService, ids);
        }
    }
}
