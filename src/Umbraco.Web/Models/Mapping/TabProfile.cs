using AutoMapper;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Mapping
{
    internal class TabProfile : Profile
    {
        public TabProfile()
        {
            CreateMap<ITag, TagModel>();
        }
    }
}
