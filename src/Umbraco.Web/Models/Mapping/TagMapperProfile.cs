using Umbraco.Core.Mapping;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Mapping
{
    internal class TagMapperProfile : IMapperProfile
    {
        public void DefineMaps(Mapper mapper)
        {
            mapper.Define<ITag, TagModel>((source, context) => new TagModel(), Map);
        }

        // Umbraco.Code.MapAll
        private static void Map(ITag source, TagModel target, MapperContext context)
        {
            target.Id = source.Id;
            target.Text = source.Text;
            target.Group = source.Group;
            target.NodeCount = source.NodeCount;
        }
    }
}
