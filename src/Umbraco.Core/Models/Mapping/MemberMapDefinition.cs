using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    public class MemberMapDefinition : IMapDefinition
    {
        public MemberMapDefinition()
        {
        }

        public void DefineMaps(UmbracoMapper mapper)
        {
            mapper.Define<MemberSave, IMember>(Map);
        }

        // mappers
        private static void Map(MemberSave source, IMember target, MapperContext context)
        {
            target.IsApproved = source.IsApproved;
            target.Name = source.Name;
            target.Email = source.Email;
            target.Key = source.Key;
            target.Username = source.Username;
            target.Id = (int)(long)source.Id;
            //TODO: map more properties as required
        }
    }
}
