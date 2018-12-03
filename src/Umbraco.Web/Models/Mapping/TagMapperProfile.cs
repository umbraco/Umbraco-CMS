using AutoMapper;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Mapping
{
    internal class TagMapperProfile : Profile
    {
        public TagMapperProfile()
        {
            CreateMap<ITag, TagModel>();
        }
    }
}
