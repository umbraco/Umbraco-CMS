using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class SectionModelMapper : ModelMapperConfiguration
    {
        private readonly ILocalizedTextService _textService;

        public SectionModelMapper(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override void ConfigureMappings(IMapperConfiguration config)
        {
            config.CreateMap<Core.Models.Section, Section>()
                .ForMember(
                      dto => dto.Name,
                      expression => expression.MapFrom(section => _textService.Localize("sections/" + section.Alias, (IDictionary<string, string>)null)))
                  .ReverseMap(); //backwards too!
        }
    }
}