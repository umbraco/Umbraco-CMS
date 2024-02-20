using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <summary>
///     A mapper which declares how to map content properties. These mappings are shared among media (and probably members)
///     which is
///     why they are in their own mapper
/// </summary>
public class ContentPropertyMapDefinition : IMapDefinition
{
    private readonly ContentPropertyBasicMapper<ContentPropertyBasic> _contentPropertyBasicConverter;
    private readonly ContentPropertyDisplayMapper _contentPropertyDisplayMapper;
    private readonly ContentPropertyDtoMapper _contentPropertyDtoConverter;
    private readonly CommonMapper _commonMapper;
    private readonly ContentBasicSavedStateMapper<ContentPropertyBasic> _basicStateMapper;


    public ContentPropertyMapDefinition(
        ICultureDictionary cultureDictionary,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        ILocalizedTextService textService,
        ILoggerFactory loggerFactory,
        PropertyEditorCollection propertyEditors,
        CommonMapper commonMapper)
    {
        _commonMapper = commonMapper;
        _basicStateMapper = new ContentBasicSavedStateMapper<ContentPropertyBasic>();;
        _contentPropertyBasicConverter = new ContentPropertyBasicMapper<ContentPropertyBasic>(
            dataTypeService,
            entityService,
            loggerFactory.CreateLogger<ContentPropertyBasicMapper<ContentPropertyBasic>>(),
            propertyEditors);
        _contentPropertyDtoConverter = new ContentPropertyDtoMapper(
            dataTypeService,
            entityService,
            loggerFactory.CreateLogger<ContentPropertyDtoMapper>(),
            propertyEditors);
        _contentPropertyDisplayMapper = new ContentPropertyDisplayMapper(
            cultureDictionary,
            dataTypeService,
            entityService,
            textService,
            loggerFactory.CreateLogger<ContentPropertyDisplayMapper>(),
            propertyEditors);
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<PropertyGroup, Tab<ContentPropertyDisplay>>(
            (source, context) => new Tab<ContentPropertyDisplay>(), Map);
        mapper.Define<IProperty, ContentPropertyBasic>((source, context) => new ContentPropertyBasic(), Map);
        mapper.Define<IProperty, ContentPropertyDto>((source, context) => new ContentPropertyDto(), Map);
        mapper.Define<IProperty, ContentPropertyDisplay>((source, context) => new ContentPropertyDisplay(), Map);
        mapper.Define<IContent, ContentItemBasic<ContentPropertyBasic>>((source, context) => new ContentItemBasic<ContentPropertyBasic>(), Map);
        mapper.Define<IContent, ContentPropertyCollectionDto>((source, context) => new ContentPropertyCollectionDto(), Map);
    }

    // Umbraco.Code.MapAll -Properties -Alias -Expanded
    private void Map(PropertyGroup source, Tab<ContentPropertyDisplay> target, MapperContext mapper)
    {
        target.Id = source.Id;
        target.Key = source.Key;
        target.Type = source.Type.ToString();
        target.Label = source.Name;
        target.Alias = source.Alias;
        target.IsActive = true;
    }

    private void Map(IProperty source, ContentPropertyBasic target, MapperContext context) =>

        // assume this is mapping everything and no MapAll is required
        _contentPropertyBasicConverter.Map(source, target, context);

    private void Map(IProperty source, ContentPropertyDto target, MapperContext context) =>

        // assume this is mapping everything and no MapAll is required
        _contentPropertyDtoConverter.Map(source, target, context);

    private void Map(IProperty source, ContentPropertyDisplay target, MapperContext context) =>

        // assume this is mapping everything and no MapAll is required
        _contentPropertyDisplayMapper.Map(source, target, context);

    // Umbraco.Code.MapAll -Alias
    private void Map(IContent source, ContentItemBasic<ContentPropertyBasic> target, MapperContext context)
    {
        target.ContentTypeId = source.ContentType.Id;
        target.ContentTypeAlias = source.ContentType.Alias;
        target.CreateDate = source.CreateDate;
        target.Edited = source.Edited;
        target.Icon = source.ContentType.Icon;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = GetName(source, context);
        target.Owner = _commonMapper.GetOwner(source, context);
        target.ParentId = source.ParentId;
        target.Path = source.Path;
        target.Properties = context.MapEnumerable<IProperty, ContentPropertyBasic>(source.Properties).WhereNotNull();
        target.SortOrder = source.SortOrder;
        target.State = _basicStateMapper.Map(source, context);
        target.Trashed = source.Trashed;
        target.Udi =
            Udi.Create(source.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, source.Key);
        target.UpdateDate = GetUpdateDate(source, context);
        target.Updater = _commonMapper.GetCreator(source, context);
        target.VariesByCulture = source.ContentType.VariesByCulture();
    }

    // Umbraco.Code.MapAll
    private static void Map(IContent source, ContentPropertyCollectionDto target, MapperContext context) =>
        target.Properties = context.MapEnumerable<IProperty, ContentPropertyDto>(source.Properties).WhereNotNull();

    private string? GetName(IContent source, MapperContext context)
    {
        // invariant = only 1 name
        if (!source.ContentType.VariesByCulture())
        {
            return source.Name;
        }

        // variant = depends on culture
        var culture = context.GetCulture();

        // if there's no culture here, the issue is somewhere else (UI, whatever) - throw!
        if (culture == null)
        {
            throw new InvalidOperationException("Missing culture in mapping options.");
        }

        // if we don't have a name for a culture, it means the culture is not available, and
        // hey we should probably not be mapping it, but it's too late, return a fallback name
        return source.CultureInfos is not null &&
               source.CultureInfos.TryGetValue(culture, out ContentCultureInfos name) && !name.Name.IsNullOrWhiteSpace()
            ? name.Name
            : $"({source.Name})";
    }

    private DateTime GetUpdateDate(IContent source, MapperContext context)
    {
        // invariant = global date
        if (!source.ContentType.VariesByCulture())
        {
            return source.UpdateDate;
        }

        // variant = depends on culture
        var culture = context.GetCulture();

        // if there's no culture here, the issue is somewhere else (UI, whatever) - throw!
        if (culture == null)
        {
            throw new InvalidOperationException("Missing culture in mapping options.");
        }

        // if we don't have a date for a culture, it means the culture is not available, and
        // hey we should probably not be mapping it, but it's too late, return a fallback date
        DateTime? date = source.GetUpdateDate(culture);
        return date ?? source.UpdateDate;
    }
}
