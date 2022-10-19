using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Mapping;

/// <summary>
///     Declares model mappings for members.
/// </summary>
public class MemberMapDefinition : IMapDefinition
{
    private readonly CommonMapper _commonMapper;
    private readonly CommonTreeNodeMapper _commonTreeNodeMapper;
    private readonly MemberTabsAndPropertiesMapper _tabsAndPropertiesMapper;

    public MemberMapDefinition(CommonMapper commonMapper, CommonTreeNodeMapper commonTreeNodeMapper,
        MemberTabsAndPropertiesMapper tabsAndPropertiesMapper)
    {
        _commonMapper = commonMapper;
        _commonTreeNodeMapper = commonTreeNodeMapper;
        _tabsAndPropertiesMapper = tabsAndPropertiesMapper;
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IMember, MemberDisplay>((source, context) => new MemberDisplay(), Map);
        mapper.Define<IMember, MemberBasic>((source, context) => new MemberBasic(), Map);
        mapper.Define<IMemberGroup, MemberGroupDisplay>((source, context) => new MemberGroupDisplay(), Map);
        mapper.Define<UmbracoIdentityRole, MemberGroupDisplay>((source, context) => new MemberGroupDisplay(), Map);
        mapper.Define<IMember, ContentPropertyCollectionDto>((source, context) => new ContentPropertyCollectionDto(),
            Map);
    }

    // Umbraco.Code.MapAll -Properties -Errors -Edited -Updater -Alias -IsChildOfListView
    // Umbraco.Code.MapAll -Trashed -IsContainer -VariesByCulture
    private void Map(IMember source, MemberDisplay target, MapperContext context)
    {
        target.ContentApps = _commonMapper.GetContentAppsForEntity(source);
        target.ContentType = _commonMapper.GetContentType(source, context);
        target.ContentTypeId = source.ContentType.Id;
        target.ContentTypeAlias = source.ContentType.Alias;
        target.ContentTypeName = source.ContentType.Name;
        target.CreateDate = source.CreateDate;
        target.Icon = source.ContentType.Icon;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.Owner = _commonMapper.GetOwner(source, context);
        target.ParentId = source.ParentId;
        target.Path = source.Path;
        target.SortOrder = source.SortOrder;
        target.State = null;
        target.Tabs = _tabsAndPropertiesMapper.Map(source, context);
        target.TreeNodeUrl = _commonTreeNodeMapper.GetTreeNodeUrl<MemberTreeController>(source);
        target.Udi = Udi.Create(Constants.UdiEntityType.Member, source.Key);
        target.UpdateDate = source.UpdateDate;

        //Membership
        target.Username = source.Username;
        target.Email = source.Email;
        target.IsLockedOut = source.IsLockedOut;
        target.IsApproved = source.IsApproved;
        target.MembershipProperties = _tabsAndPropertiesMapper.MapMembershipProperties(source, context);
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
    }

    // Umbraco.Code.MapAll -Icon -Trashed -ParentId -Alias
    private void Map(IMemberGroup source, MemberGroupDisplay target, MapperContext context)
    {
        target.Icon = Constants.Icons.MemberGroup;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.Path = $"-1,{source.Id}";
        target.Udi = Udi.Create(Constants.UdiEntityType.MemberGroup, source.Key);
    }

    // Umbraco.Code.MapAll -Icon -Trashed -ParentId -Alias -Key -Udi
    private void Map(UmbracoIdentityRole source, MemberGroupDisplay target, MapperContext context)
    {
        target.Id = source.Id;
        //target.Key = source.Key;
        target.Name = source.Name;
        target.Path = $"-1,{source.Id}";
        //target.Udi = Udi.Create(Constants.UdiEntityType.MemberGroup, source.Key);
    }

    // Umbraco.Code.MapAll
    private static void Map(IMember source, ContentPropertyCollectionDto target, MapperContext context) =>
        target.Properties = context.MapEnumerable<IProperty, ContentPropertyDto>(source.Properties).WhereNotNull();
}
