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
                var allLanguages = source.OrderBy(x => x.Id).ToList();
                var langs = new List<Language>(allLanguages.Select(x => context.Mapper.Map<ILanguage, Language>(x, null, context)));

                //if there's only one language, by default it is the default
                if (langs.Count == 1)
                {
                    langs[0].IsDefaultVariantLanguage = true;
                    langs[0].Mandatory = true;
                }
                else if (allLanguages.All(x => !x.IsDefaultVariantLanguage))
                {
                    //if no language has the default flag, then the defaul language is the one with the lowest id
                    langs[0].IsDefaultVariantLanguage = true;
                    langs[0].Mandatory = true;
                }

                return langs.OrderBy(x => x.Name);
            }
        }
    }
}
