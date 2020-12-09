using Umbraco.Core.Mapping;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Core.Models.Mapping
{
    /// <inheritdoc />
    public class MemberMapDefinition : IMapDefinition
    {
        /// <inheritdoc />
        public void DefineMaps(UmbracoMapper mapper) => mapper.Define<MemberSave, IMember>(Map);

        //TODO: put this here instead of a new mapper definition (like user). Can move

        private static void Map(MemberSave source, IMember target, MapperContext context)
        {
            // TODO: ensure all properties are mapped as required
            target.IsApproved = source.IsApproved;
            target.Name = source.Name;
            target.Email = source.Email;
            target.Key = source.Key;
            target.Username = source.Username;
            target.Comments = source.Comments;
            //TODO: add groups as required
        }
    }
}
