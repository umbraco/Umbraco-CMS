using System.Globalization;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class LanguageMapperProfile : Profile
    {
        public LanguageMapperProfile()
        {
            CreateMap<ILanguage, LanguageDisplay>()
                .ForMember(l => l.Name, expression => expression.MapFrom(x => x.CultureInfo.DisplayName));

            CreateMap<CultureInfo, Culture>()
                .ForMember(c => c.Name, expression => expression.MapFrom(x => x.DisplayName))
                .ForMember(c => c.IsoCode, expression => expression.MapFrom(x => x.Name));
        }
    }
}
