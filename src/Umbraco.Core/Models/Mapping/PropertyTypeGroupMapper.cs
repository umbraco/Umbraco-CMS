using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

public class PropertyTypeGroupMapper<TPropertyType>
    where TPropertyType : PropertyTypeDisplay, new()
{
    private readonly IDataTypeService _dataTypeService;
    private readonly ILogger<PropertyTypeGroupMapper<TPropertyType>> _logger;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IShortStringHelper _shortStringHelper;

    public PropertyTypeGroupMapper(PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IShortStringHelper shortStringHelper, ILogger<PropertyTypeGroupMapper<TPropertyType>> logger)
    {
        _propertyEditors = propertyEditors;
        _dataTypeService = dataTypeService;
        _shortStringHelper = shortStringHelper;
        _logger = logger;
    }

    public IEnumerable<PropertyGroupDisplay<TPropertyType>> Map(IContentTypeComposition source)
    {
        // deal with groups
        var groups = new List<PropertyGroupDisplay<TPropertyType>>();

        // add groups local to this content type
        foreach (PropertyGroup propertyGroup in source.PropertyGroups)
        {
            var group = new PropertyGroupDisplay<TPropertyType>
            {
                Id = propertyGroup.Id,
                Key = propertyGroup.Key,
                Type = propertyGroup.Type,
                Name = propertyGroup.Name,
                Alias = propertyGroup.Alias,
                SortOrder = propertyGroup.SortOrder,
                Properties = MapProperties(propertyGroup.PropertyTypes, source, propertyGroup.Id, false),
                ContentTypeId = source.Id,
            };

            groups.Add(group);
        }

        // add groups inherited through composition
        var localGroupIds = groups.Select(x => x.Id).ToArray();
        foreach (PropertyGroup propertyGroup in source.CompositionPropertyGroups)
        {
            // skip those that are local to this content type
            if (localGroupIds.Contains(propertyGroup.Id))
            {
                continue;
            }

            // get the content type that defines this group
            IContentTypeComposition? definingContentType = GetContentTypeForPropertyGroup(source, propertyGroup.Id);
            if (definingContentType == null)
            {
                throw new Exception("PropertyGroup with id=" + propertyGroup.Id +
                                    " was not found on any of the content type's compositions.");
            }

            var group = new PropertyGroupDisplay<TPropertyType>
            {
                Inherited = true,
                Id = propertyGroup.Id,
                Key = propertyGroup.Key,
                Type = propertyGroup.Type,
                Name = propertyGroup.Name,
                Alias = propertyGroup.Alias,
                SortOrder = propertyGroup.SortOrder,
                Properties =
                    MapProperties(propertyGroup.PropertyTypes, definingContentType, propertyGroup.Id, true),
                ContentTypeId = definingContentType.Id,
                ParentTabContentTypes = new[] { definingContentType.Id },
                ParentTabContentTypeNames = new[] { definingContentType.Name },
            };

            groups.Add(group);
        }

        // deal with generic properties
        var genericProperties = new List<TPropertyType>();

        // add generic properties local to this content type
        IEnumerable<IPropertyType> entityGenericProperties = source.PropertyTypes.Where(x => x.PropertyGroupId == null);
        genericProperties.AddRange(MapProperties(entityGenericProperties, source, PropertyGroupBasic.GenericPropertiesGroupId, false));

        // add generic properties inherited through compositions
        var localGenericPropertyIds = genericProperties.Select(x => x.Id).ToArray();
        IEnumerable<IPropertyType> compositionGenericProperties = source.CompositionPropertyTypes
            .Where(x => x.PropertyGroupId == null // generic
                        && localGenericPropertyIds.Contains(x.Id) == false); // skip those that are local
        foreach (IPropertyType compositionGenericProperty in compositionGenericProperties)
        {
            IContentTypeComposition? definingContentType =
                GetContentTypeForPropertyType(source, compositionGenericProperty.Id);
            if (definingContentType == null)
            {
                throw new Exception("PropertyType with id=" + compositionGenericProperty.Id +
                                    " was not found on any of the content type's compositions.");
            }

            genericProperties.AddRange(MapProperties(new[] { compositionGenericProperty }, definingContentType, PropertyGroupBasic.GenericPropertiesGroupId, true));
        }

        // if there are any generic properties, add the corresponding tab
        if (genericProperties.Any())
        {
            var genericGroup = new PropertyGroupDisplay<TPropertyType>
            {
                Id = PropertyGroupBasic.GenericPropertiesGroupId,
                Name = "Generic properties",
                Alias = "genericProperties",
                SortOrder = 999,
                Properties = genericProperties,
                ContentTypeId = source.Id,
            };

            groups.Add(genericGroup);
        }

        // handle locked properties
        var lockedPropertyAliases = new List<string>();

        // add built-in member property aliases to list of aliases to be locked
        foreach (var propertyAlias in ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper).Keys)
        {
            lockedPropertyAliases.Add(propertyAlias);
        }

        // lock properties by aliases
        foreach (TPropertyType property in groups.SelectMany(x => x.Properties))
        {
            if (property.Alias is not null)
            {
                property.Locked = lockedPropertyAliases.Contains(property.Alias);
            }
        }

        // now merge tabs based on alias
        // as for one name, we might have one local tab, plus some inherited tabs
        IGrouping<string, PropertyGroupDisplay<TPropertyType>>[] groupsGroupsByAlias =
            groups.GroupBy(x => x.Alias).ToArray();
        groups = new List<PropertyGroupDisplay<TPropertyType>>(); // start with a fresh list
        foreach (IGrouping<string, PropertyGroupDisplay<TPropertyType>> groupsByAlias in groupsGroupsByAlias)
        {
            // single group, just use it
            if (groupsByAlias.Count() == 1)
            {
                groups.Add(groupsByAlias.First());
                continue;
            }

            // multiple groups, merge
            PropertyGroupDisplay<TPropertyType> group =
                groupsByAlias.FirstOrDefault(x => x.Inherited == false) // try local
                ?? groupsByAlias.First(); // else pick one randomly
            groups.Add(group);

            // in case we use the local one, flag as inherited
            group.Inherited =
                true; // TODO Remove to allow changing sort order of the local one (and use the inherited group order below)

            // merge (and sort) properties
            TPropertyType[] properties =
                groupsByAlias.SelectMany(x => x.Properties).OrderBy(x => x.SortOrder).ToArray();
            group.Properties = properties;

            // collect parent group info
            PropertyGroupDisplay<TPropertyType>[] parentGroups =
                groupsByAlias.Where(x => x.ContentTypeId != source.Id).ToArray();
            group.ParentTabContentTypes = parentGroups.SelectMany(x => x.ParentTabContentTypes).ToArray();
            group.ParentTabContentTypeNames = parentGroups.SelectMany(x => x.ParentTabContentTypeNames).ToArray();
        }

        return groups.OrderBy(x => x.SortOrder);
    }

    /// <summary>
    ///     Gets the content type that defines a property group, within a composition.
    /// </summary>
    /// <param name="contentType">The composition.</param>
    /// <param name="propertyGroupId">The identifier of the property group.</param>
    /// <returns>The composition content type that defines the specified property group.</returns>
    private static IContentTypeComposition? GetContentTypeForPropertyGroup(
        IContentTypeComposition contentType,
        int propertyGroupId)
    {
        // test local groups
        if (contentType.PropertyGroups.Any(x => x.Id == propertyGroupId))
        {
            return contentType;
        }

        // test composition types groups
        // .ContentTypeComposition is just the local ones, not recursive,
        // so we have to recurse here
        return contentType.ContentTypeComposition
            .Select(x => GetContentTypeForPropertyGroup(x, propertyGroupId))
            .FirstOrDefault(x => x != null);
    }

    /// <summary>
    ///     Gets the content type that defines a property group, within a composition.
    /// </summary>
    /// <param name="contentType">The composition.</param>
    /// <param name="propertyTypeId">The identifier of the property type.</param>
    /// <returns>The composition content type that defines the specified property group.</returns>
    private static IContentTypeComposition? GetContentTypeForPropertyType(
        IContentTypeComposition contentType,
        int propertyTypeId)
    {
        // test local property types
        if (contentType.PropertyTypes.Any(x => x.Id == propertyTypeId))
        {
            return contentType;
        }

        // test composition property types
        // .ContentTypeComposition is just the local ones, not recursive,
        // so we have to recurse here
        return contentType.ContentTypeComposition
            .Select(x => GetContentTypeForPropertyType(x, propertyTypeId))
            .FirstOrDefault(x => x != null);
    }

    private IEnumerable<TPropertyType> MapProperties(
        IEnumerable<IPropertyType>? properties,
        IContentTypeBase contentType,
        int groupId,
        bool inherited)
    {
        var mappedProperties = new List<TPropertyType>();

        foreach (IPropertyType p in properties?.Where(x => x.DataTypeId != 0).OrderBy(x => x.SortOrder) ??
                                    Enumerable.Empty<IPropertyType>())
        {
            var propertyEditorAlias = p.PropertyEditorAlias;
            IDataEditor? propertyEditor = _propertyEditors[propertyEditorAlias];
            IDataType? dataType = _dataTypeService.GetDataType(p.DataTypeId);

            // fixme: Don't explode if we can't find this, log an error and change this to a label
            if (propertyEditor == null)
            {
                _logger.LogError(
                    "No property editor could be resolved with the alias: {PropertyEditorAlias}, defaulting to label",
                    p.PropertyEditorAlias);
                propertyEditorAlias = Constants.PropertyEditors.Aliases.Label;
                propertyEditor = _propertyEditors[propertyEditorAlias];
            }

            IDictionary<string, object>? config = propertyEditor is null || dataType is null ? new Dictionary<string, object>()
                : dataType.Editor?.GetConfigurationEditor().ToConfigurationEditor(dataType.Configuration);

            mappedProperties.Add(new TPropertyType
            {
                Id = p.Id,
                Alias = p.Alias,
                Description = p.Description,
                LabelOnTop = p.LabelOnTop,
                Editor = p.PropertyEditorAlias,
                Validation = new PropertyTypeValidation
                {
                    Mandatory = p.Mandatory,
                    MandatoryMessage = p.MandatoryMessage,
                    Pattern = p.ValidationRegExp,
                    PatternMessage = p.ValidationRegExpMessage,
                },
                Label = p.Name,
                View = propertyEditor?.GetValueEditor().View,
                Config = config,

                // Value = "",
                GroupId = groupId,
                Inherited = inherited,
                DataTypeId = p.DataTypeId,
                DataTypeKey = p.DataTypeKey,
                DataTypeName = dataType?.Name,
                DataTypeIcon = propertyEditor?.Icon,
                SortOrder = p.SortOrder,
                ContentTypeId = contentType.Id,
                ContentTypeName = contentType.Name,
                AllowCultureVariant = p.VariesByCulture(),
                AllowSegmentVariant = p.VariesBySegment(),
            });
        }

        return mappedProperties;
    }
}
