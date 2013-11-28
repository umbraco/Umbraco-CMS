using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;
using umbraco;

namespace Umbraco.Web.Models.Mapping
{
    internal class SectionModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<Core.Models.Section, Section>()
                .ForMember(
                      dto => dto.Name,
                      expression => expression.MapFrom(section => ui.Text("sections", section.Alias)))
                  .ReverseMap(); //backwards too!
        }
    }
}