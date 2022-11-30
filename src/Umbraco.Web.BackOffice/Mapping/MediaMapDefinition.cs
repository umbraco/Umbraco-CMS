using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Mapping;

/// <summary>
///     Declares model mappings for media.
/// </summary>
public class MediaMapDefinition : IMapDefinition
{
    private readonly CommonMapper _commonMapper;
    private readonly CommonTreeNodeMapper _commonTreeNodeMapper;
    private readonly ContentSettings _contentSettings;
    private readonly IMediaService _mediaService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly TabsAndPropertiesMapper<IMedia> _tabsAndPropertiesMapper;

    public MediaMapDefinition(ICultureDictionary cultureDictionary, CommonMapper commonMapper,
        CommonTreeNodeMapper commonTreeNodeMapper, IMediaService mediaService, IMediaTypeService mediaTypeService,
        ILocalizedTextService localizedTextService, MediaUrlGeneratorCollection mediaUrlGenerators,
        IOptions<ContentSettings> contentSettings, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
    {
        _commonMapper = commonMapper;
        _commonTreeNodeMapper = commonTreeNodeMapper;
        _mediaService = mediaService;
        _mediaTypeService = mediaTypeService;
        _mediaUrlGenerators = mediaUrlGenerators;
        _contentSettings = contentSettings.Value ?? throw new ArgumentNullException(nameof(contentSettings));

        _tabsAndPropertiesMapper =
            new TabsAndPropertiesMapper<IMedia>(cultureDictionary, localizedTextService,
                contentTypeBaseServiceProvider);
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IMedia, ContentPropertyCollectionDto>((source, context) => new ContentPropertyCollectionDto(),
            Map);
        mapper.Define<IMedia, MediaItemDisplay>((source, context) => new MediaItemDisplay(), Map);
        mapper.Define<IMedia, ContentItemBasic<ContentPropertyBasic>>(
            (source, context) => new ContentItemBasic<ContentPropertyBasic>(), Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(IMedia source, ContentPropertyCollectionDto target, MapperContext context) =>
        target.Properties = context.MapEnumerable<IProperty, ContentPropertyDto>(source.Properties).WhereNotNull();

    // Umbraco.Code.MapAll -Properties -Errors -Edited -Updater -Alias -IsContainer
    private void Map(IMedia source, MediaItemDisplay target, MapperContext context)
    {
        target.ContentApps = _commonMapper.GetContentAppsForEntity(source);
        target.ContentType = _commonMapper.GetContentType(source, context);
        target.ContentTypeId = source.ContentType.Id;
        target.ContentTypeAlias = source.ContentType.Alias;
        target.ContentTypeName = source.ContentType.Name;
        target.CreateDate = source.CreateDate;
        target.Icon = source.ContentType.Icon;
        target.Id = source.Id;
        target.IsChildOfListView = DetermineIsChildOfListView(source);
        target.Key = source.Key;
        target.MediaLink = string.Join(",", source.GetUrls(_contentSettings, _mediaUrlGenerators));
        target.Name = source.Name;
        target.Owner = _commonMapper.GetOwner(source, context);
        target.ParentId = source.ParentId;
        target.Path = source.Path;
        target.SortOrder = source.SortOrder;
        target.State = null;
        target.Tabs = _tabsAndPropertiesMapper.Map(source, context);
        target.Trashed = source.Trashed;
        target.TreeNodeUrl = _commonTreeNodeMapper.GetTreeNodeUrl<MediaTreeController>(source);
        target.Udi = Udi.Create(Constants.UdiEntityType.Media, source.Key);
        target.UpdateDate = source.UpdateDate;
        target.VariesByCulture = source.ContentType.VariesByCulture();
    }

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

    private bool DetermineIsChildOfListView(IMedia source)
    {
        // map the IsChildOfListView (this is actually if it is a descendant of a list view!)
        IMedia? parent = _mediaService.GetParent(source);
        return parent != null && (parent.ContentType.IsContainer || _mediaTypeService.HasContainerInPath(parent.Path));
    }
}
