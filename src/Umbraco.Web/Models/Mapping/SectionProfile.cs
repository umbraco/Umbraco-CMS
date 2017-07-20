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
                .ForMember(
                      dto => dto.Name,
                      expression => expression.MapFrom(section => _textService.Localize("sections/" + section.Alias, (IDictionary<string, string>)null)))
                  .ReverseMap(); //backwards too!
        }
    }
}
