using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;
using umbraco;

namespace Umbraco.Web.Models.Mapping
{
    internal class SectionModelMapper : ModelMapperConfiguration
    {
        public override void ConfigureMappings(IMapperConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<Core.Models.Section, Section>()
                .ForMember(
                      dto => dto.Name,
                      expression => expression.MapFrom(section => applicationContext.Services.TextService.Localize("sections/" + section.Alias, (IDictionary<string, string>)null)))
                  .ReverseMap(); //backwards too!
        }
    }
}