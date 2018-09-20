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
                return source.Select(x => context.Mapper.Map<ILanguage, Language>(x, null, context)).OrderBy(x => x.Name);
            }
        }
    }
}
