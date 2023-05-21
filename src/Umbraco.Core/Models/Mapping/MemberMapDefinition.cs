using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <inheritdoc />
public class MemberMapDefinition : IMapDefinition
{
    /// <inheritdoc />
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<MemberSave, IMember>(Map);

    private static void Map(MemberSave source, IMember target, MapperContext context)
    {
        target.IsApproved = source.IsApproved;
        target.Name = source.Name;
        target.Email = source.Email;
        target.Key = source.Key;
        target.Username = source.Username;
        target.Comments = source.Comments;
        target.CreateDate = source.CreateDate;
        target.UpdateDate = source.UpdateDate;
        target.Email = source.Email;

        // TODO: ensure all properties are mapped as required
        // target.Id = source.Id;
        // target.ParentId = -1;
        // target.Path = "-1," + source.Id;

        // TODO: add groups as required
    }
}
