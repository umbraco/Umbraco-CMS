using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class SectionProfile : Profile
    {
        private readonly ILocalizedTextService _textService;

        public SectionProfile(ILocalizedTextService textService)
        {
            _textService = textService;

            CreateMap<Core.Models.Section, Section>()
                .ForMember(dest => dest.RoutePath, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => _textService.Localize("sections/" + src.Alias, (IDictionary<string, string>)null)))
                  .ReverseMap(); //backwards too!
        }
    }
}
