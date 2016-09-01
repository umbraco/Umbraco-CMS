using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.Web.Models.Mapping
{
    internal class TabModelMapper : ModelMapperConfiguration
    {
        public override void ConfigureMappings(IMapperConfiguration config)
        {
            config.CreateMap<ITag, TagModel>();
        }
    }
}