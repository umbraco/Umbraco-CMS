using AutoMapper;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class SectionModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config)
        {
            config.CreateMap<Section, Core.Models.Section>()
                  .ReverseMap(); //backwards too!
        }
    }
}