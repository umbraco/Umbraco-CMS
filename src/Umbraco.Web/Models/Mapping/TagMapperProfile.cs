using Umbraco.Core.Mapping;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Mapping
{
    internal class TagMapperProfile : IMapperProfile
    {
        public void SetMaps(Mapper mapper)
        {
            mapper.Define<ITag, TagModel>(source => new TagModel(), Map);
        }

        // Umbraco.Code.MapAll
        private static void Map(ITag source, TagModel target)
        {
            target.Id = source.Id;
            target.Text = source.Text;
            target.Group = source.Group;
            target.NodeCount = source.NodeCount;
        }
    }
}
