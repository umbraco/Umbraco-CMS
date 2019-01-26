using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Language = Umbraco.Web.Models.ContentEditing.Language;

namespace Umbraco.Web.Models.Mapping
{
    internal class LanguageMapperProfile : Profile
    {
        public LanguageMapperProfile()
        {
            CreateMap<ILanguage, EntityBasic>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(x => x.CultureName))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(x => x.Key))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(x => x.IsoCode))
                .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore());

            CreateMap<ILanguage, Language>()
                .ForMember(l => l.Name, expression => expression.MapFrom(x => x.CultureInfo.DisplayName));

            CreateMap<IEnumerable<ILanguage>, IEnumerable<Language>>()
                .ConvertUsing<LanguageCollectionTypeConverter>();

        }

        /// <summary>
        /// Converts a list of <see cref="ILanguage"/> to a list of <see cref="Language"/> and ensures the correct order and defaults are set
        /// </summary>
        // ReSharper disable once ClassNeverInstantiated.Local
        private class LanguageCollectionTypeConverter : ITypeConverter<IEnumerable<ILanguage>, IEnumerable<Language>>
        {
            public IEnumerable<Language> Convert(IEnumerable<ILanguage> source, IEnumerable<Language> destination, ResolutionContext context)
            {
                var langs = source.Select(x => context.Mapper.Map<ILanguage, Language>(x, null, context)).ToList();

                //Put the default language first in the list & then sort rest by a-z
                var defaultLang = langs.SingleOrDefault(x => x.IsDefault);

                //Remove the default language from the list for now
                langs.Remove(defaultLang);

                //Sort the remaining languages a-z
                langs = langs.OrderBy(x => x.Name).ToList();

                //Insert the default language as the first item
                langs.Insert(0, defaultLang);

                return langs;
            }
        }
    }
}
