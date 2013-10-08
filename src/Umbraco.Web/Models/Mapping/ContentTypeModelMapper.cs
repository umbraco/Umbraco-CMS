using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Defines mappings for content/media/members type mappings
    /// </summary>
    internal class ContentTypeModelMapper : MapperConfiguration
    {    
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IMediaType, ContentTypeBasic>();
            config.CreateMap<IContentType, ContentTypeBasic>();
            config.CreateMap<IMemberType, ContentTypeBasic>();
        }
    }
}