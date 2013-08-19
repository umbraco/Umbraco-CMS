using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class EntityModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<UmbracoEntity, EntityBasic>()
                  .ForMember(basic => basic.Icon, expression => expression.MapFrom(entity => entity.ContentTypeIcon));
        }
    }
}