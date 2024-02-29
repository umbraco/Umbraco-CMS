using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <summary>
///     Declares model mappings for media.
/// </summary>
public class MediaMapDefinition : IMapDefinition
{
    private readonly CommonMapper _commonMapper;

    public MediaMapDefinition(CommonMapper commonMapper)
    {
        _commonMapper = commonMapper;
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IMedia, ContentPropertyCollectionDto>((source, context) => new ContentPropertyCollectionDto(), Map);
        mapper.Define<IMedia, ContentItemBasic<ContentPropertyBasic>>((source, context) => new ContentItemBasic<ContentPropertyBasic>(), Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(IMedia source, ContentPropertyCollectionDto target, MapperContext context) =>
        target.Properties = context.MapEnumerable<IProperty, ContentPropertyDto>(source.Properties).WhereNotNull();

    // Umbraco.Code.MapAll -Edited -Updater -Alias
    private void Map(IMedia source, ContentItemBasic<ContentPropertyBasic> target, MapperContext context)
    {
        target.ContentTypeId = source.ContentType.Id;
        target.ContentTypeAlias = source.ContentType.Alias;
        target.CreateDate = source.CreateDate;
        target.Icon = source.ContentType.Icon;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.Owner = _commonMapper.GetOwner(source, context);
        target.ParentId = source.ParentId;
        target.Path = source.Path;
        target.Properties = context.MapEnumerable<IProperty, ContentPropertyBasic>(source.Properties).WhereNotNull();
        target.SortOrder = source.SortOrder;
        target.State = null;
        target.Trashed = source.Trashed;
        target.Udi = Udi.Create(Constants.UdiEntityType.Media, source.Key);
        target.UpdateDate = source.UpdateDate;
        target.VariesByCulture = source.ContentType.VariesByCulture();
    }
}
