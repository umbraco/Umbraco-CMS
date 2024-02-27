using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <inheritdoc />
public class MemberMapDefinition : IMapDefinition
{
    private readonly CommonMapper _commonMapper;

    public MemberMapDefinition(CommonMapper commonMapper) => _commonMapper = commonMapper;

    /// <inheritdoc />
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<MemberSave, IMember>(Map);
        mapper.Define<IMember, MemberBasic>((source, context) => new MemberBasic(), Map);
        mapper.Define<IMember, ContentPropertyCollectionDto>((source, context) => new ContentPropertyCollectionDto(), Map);
    }

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

    // Umbraco.Code.MapAll -Trashed -Edited -Updater -Alias -VariesByCulture
    private void Map(IMember source, MemberBasic target, MapperContext context)
    {
        target.ContentTypeId = source.ContentType.Id;
        target.ContentTypeAlias = source.ContentType.Alias;
        target.CreateDate = source.CreateDate;
        target.Email = source.Email;
        target.Icon = source.ContentType.Icon;
        target.Id = int.MaxValue;
        target.Key = source.Key;
        target.Name = source.Name;
        target.Owner = _commonMapper.GetOwner(source, context);
        target.ParentId = source.ParentId;
        target.Path = source.Path;
        target.Properties = context.MapEnumerable<IProperty, ContentPropertyBasic>(source.Properties).WhereNotNull();
        target.SortOrder = source.SortOrder;
        target.State = null;
        target.Udi = Udi.Create(Constants.UdiEntityType.Member, source.Key);
        target.UpdateDate = source.UpdateDate;
        target.Username = source.Username;
        target.FailedPasswordAttempts = source.FailedPasswordAttempts;
        target.Approved = source.IsApproved;
        target.LockedOut = source.IsLockedOut;
        target.LastLockoutDate = source.LastLockoutDate;
        target.LastLoginDate = source.LastLoginDate;
        target.LastPasswordChangeDate = source.LastPasswordChangeDate;
    }

    // Umbraco.Code.MapAll
    private static void Map(IMember source, ContentPropertyCollectionDto target, MapperContext context) =>
        target.Properties = context.MapEnumerable<IProperty, ContentPropertyDto>(source.Properties).WhereNotNull();
}
