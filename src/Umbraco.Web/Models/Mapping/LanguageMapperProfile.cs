using System.Globalization;
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
        }
    }
}
