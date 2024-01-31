﻿using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.ContentType;

public abstract class ContentTypeMapDefinition<TContentType, TPropertyTypeModel, TPropertyTypeContainerModel>
    where TContentType : IContentTypeBase
    where TPropertyTypeModel : PropertyTypeModelBase, new()
    where TPropertyTypeContainerModel : PropertyTypeContainerModelBase, new()
{
    protected IEnumerable<TPropertyTypeModel> MapPropertyTypes(TContentType source)
    {
        // create a mapping table between properties and their associated groups
        var groupKeysByPropertyKeys = source
            .PropertyGroups
            .SelectMany(propertyGroup => (propertyGroup.PropertyTypes?.ToArray() ?? Array.Empty<PropertyType>())
                .Select(propertyType => new { GroupKey = propertyGroup.Key, PropertyTypeKey = propertyType.Key }))
            .ToDictionary(map => map.PropertyTypeKey, map => map.GroupKey);

        return source.PropertyTypes.Select(propertyType =>
                new TPropertyTypeModel
                {
                    Id = propertyType.Key,
                    SortOrder = propertyType.SortOrder,
                    Container = groupKeysByPropertyKeys.ContainsKey(propertyType.Key)
                        ? new ReferenceByIdModel(groupKeysByPropertyKeys[propertyType.Key])
                        : null,
                    Name = propertyType.Name,
                    Alias = propertyType.Alias,
                    Description = propertyType.Description,
                    DataType = new ReferenceByIdModel(propertyType.DataTypeKey),
                    VariesByCulture = propertyType.VariesByCulture(),
                    VariesBySegment = propertyType.VariesBySegment(),
                    Validation = new PropertyTypeValidation
                    {
                        Mandatory = propertyType.Mandatory,
                        MandatoryMessage = propertyType.MandatoryMessage,
                        RegEx = propertyType.ValidationRegExp,
                        RegExMessage = propertyType.ValidationRegExpMessage
                    },
                    Appearance = new PropertyTypeAppearance
                    {
                        LabelOnTop = propertyType.LabelOnTop
                    }
                })
            .ToArray();
    }

    protected IEnumerable<TPropertyTypeContainerModel> MapPropertyTypeContainers(TContentType source)
    {
        // create a mapping table between property group aliases and keys
        var groupKeysByGroupAliases = source
            .PropertyGroups
            .ToDictionary(propertyGroup => propertyGroup.Alias, propertyGroup => propertyGroup.Key);

        ReferenceByIdModel? ParentGroup(PropertyGroup group)
        {
            var path = group.Alias.Split(Constants.CharArrays.ForwardSlash);
            return path.Length == 1 || groupKeysByGroupAliases.TryGetValue(path.First(), out Guid parentGroupKey) == false
                ? null
                : new ReferenceByIdModel(parentGroupKey);
        }

        return source
            .PropertyGroups
            .Select(propertyGroup =>
                new TPropertyTypeContainerModel
                {
                    Id = propertyGroup.Key,
                    Parent = ParentGroup(propertyGroup),
                    Type = propertyGroup.Type.ToString(),
                    SortOrder = propertyGroup.SortOrder,
                    Name = propertyGroup.Name ?? "-",
                })
            .ToArray();
    }

    protected static CompositionType CalculateCompositionType(TContentType source, IContentTypeComposition contentTypeComposition)
        => contentTypeComposition.Id == source.ParentId
            ? CompositionType.Inheritance
            : CompositionType.Composition;
}
