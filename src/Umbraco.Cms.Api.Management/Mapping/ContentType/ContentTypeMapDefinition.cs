using Umbraco.Cms.Api.Management.ViewModels.ContentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using ContentTypeSort = Umbraco.Cms.Api.Management.ViewModels.ContentType.ContentTypeSort;

namespace Umbraco.Cms.Api.Management.Mapping.ContentType;

public abstract class ContentTypeMapDefinition<TContentType, TPropertyTypeResponseModel, TPropertyTypeContainerResponseModel>
    where TContentType : IContentTypeBase
    where TPropertyTypeResponseModel : PropertyTypeResponseModelBase, new()
    where TPropertyTypeContainerResponseModel : PropertyTypeContainerResponseModelBase, new()
{
    protected IEnumerable<TPropertyTypeResponseModel> MapPropertyTypes(TContentType source)
    {
        // create a mapping table between properties and their associated groups
        var groupKeysByPropertyKeys = source
            .PropertyGroups
            .SelectMany(propertyGroup => (propertyGroup.PropertyTypes?.ToArray() ?? Array.Empty<PropertyType>())
                .Select(propertyType => new { GroupKey = propertyGroup.Key, PropertyTypeKey = propertyType.Key }))
            .ToDictionary(map => map.PropertyTypeKey, map => map.GroupKey);

        return source.PropertyTypes.Select(propertyType =>
                new TPropertyTypeResponseModel
                {
                    Id = propertyType.Key,
                    SortOrder = propertyType.SortOrder,
                    ContainerId = groupKeysByPropertyKeys.ContainsKey(propertyType.Key)
                        ? groupKeysByPropertyKeys[propertyType.Key]
                        : null,
                    Name = propertyType.Name,
                    Alias = propertyType.Alias,
                    Description = propertyType.Description,
                    DataTypeId = propertyType.DataTypeKey,
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

    protected IEnumerable<TPropertyTypeContainerResponseModel> MapPropertyTypeContainers(TContentType source)
    {
        // create a mapping table between property group aliases and keys
        var groupKeysByGroupAliases = source
            .PropertyGroups
            .ToDictionary(propertyGroup => propertyGroup.Alias, propertyGroup => propertyGroup.Key);

        Guid? ParentGroupKey(PropertyGroup group)
        {
            var path = group.Alias.Split(Constants.CharArrays.ForwardSlash);
            return path.Length == 1 || groupKeysByGroupAliases.TryGetValue(path.First(), out Guid parentGroupKey) == false
                ? null
                : parentGroupKey;
        }

        return source
            .PropertyGroups
            .Select(propertyGroup =>
                new TPropertyTypeContainerResponseModel
                {
                    Id = propertyGroup.Key,
                    ParentId = ParentGroupKey(propertyGroup),
                    Type = propertyGroup.Type.ToString(),
                    SortOrder = propertyGroup.SortOrder,
                    Name = propertyGroup.Name,
                })
            .ToArray();
    }

    protected IEnumerable<ContentTypeSort> MapAllowedContentTypes(TContentType source)
        => source.AllowedContentTypes?.Select(contentTypeSort => new ContentTypeSort { Id = contentTypeSort.Key, SortOrder = contentTypeSort.SortOrder }).ToArray()
           ?? Array.Empty<ContentTypeSort>();

    protected IEnumerable<ContentTypeComposition> MapCompositions(TContentType source, IEnumerable<IContentTypeComposition> contentTypeComposition)
        => contentTypeComposition.Select(contentType => new ContentTypeComposition
        {
            Id = contentType.Key,
            CompositionType = contentType.Id == source.ParentId
                ? ContentTypeCompositionType.Inheritance
                : ContentTypeCompositionType.Composition
        }).ToArray();
}
