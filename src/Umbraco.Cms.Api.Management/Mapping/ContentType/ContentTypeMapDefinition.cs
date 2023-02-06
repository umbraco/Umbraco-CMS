using Umbraco.Cms.Api.Management.ViewModels.ContentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.ContentType;

public abstract class ContentTypeMapDefinition<TContentType, TPropertyTypeViewModel, TPropertyTypeContainerViewModel>
    where TContentType : IContentTypeBase
    where TPropertyTypeViewModel : PropertyTypeViewModelBase, new()
    where TPropertyTypeContainerViewModel : PropertyTypeContainerViewModelBase, new()
{
    protected IEnumerable<TPropertyTypeViewModel> MapPropertyTypes(TContentType source)
    {
        // create a mapping table between properties and their associated groups
        var groupKeysByPropertyKeys = source
            .PropertyGroups
            .SelectMany(propertyGroup => (propertyGroup.PropertyTypes?.ToArray() ?? Array.Empty<PropertyType>())
                .Select(propertyType => new { GroupKey = propertyGroup.Key, PropertyTypeKey = propertyType.Key }))
            .ToDictionary(map => map.PropertyTypeKey, map => map.GroupKey);

        return source.PropertyTypes.Select(propertyType =>
                new TPropertyTypeViewModel
                {
                    Key = propertyType.Key,
                    ContainerKey = groupKeysByPropertyKeys.ContainsKey(propertyType.Key)
                        ? groupKeysByPropertyKeys[propertyType.Key]
                        : null,
                    Name = propertyType.Name,
                    Alias = propertyType.Alias,
                    Description = propertyType.Description,
                    DataTypeKey = propertyType.DataTypeKey,
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

    protected IEnumerable<TPropertyTypeContainerViewModel> MapPropertyTypeContainers(TContentType source)
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
                new TPropertyTypeContainerViewModel
                {
                    Key = propertyGroup.Key,
                    ParentKey = ParentGroupKey(propertyGroup),
                    Type = propertyGroup.Type.ToString(),
                    SortOrder = propertyGroup.SortOrder,
                    Name = propertyGroup.Name,
                })
            .ToArray();
    }
}
