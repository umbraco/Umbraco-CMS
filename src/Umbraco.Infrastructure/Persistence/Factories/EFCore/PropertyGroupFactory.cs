using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories.EFCore;

internal static class PropertyGroupFactory
{
    /// <summary>
    /// Creates a <see cref="PropertyTypeGroupDto"/> from the specified property group.
    /// The identity is never set: inserts rely on the database-generated id, and updates are performed with
    /// set-based statements that take the target id from the entity.
    /// </summary>
    internal static PropertyTypeGroupDto BuildGroupDto(PropertyGroup propertyGroup, int contentTypeId) => new()
    {
        UniqueId = propertyGroup.Key,
        Type = (short)propertyGroup.Type,
        ContentTypeNodeId = contentTypeId,
        Text = propertyGroup.Name,
        Alias = propertyGroup.Alias,
        SortOrder = propertyGroup.SortOrder,
    };

    /// <summary>
    /// Creates a <see cref="PropertyTypeDto"/> from the specified property type.
    /// The identity is never set: inserts rely on the database-generated id, and updates are performed with
    /// set-based statements that take the target id from the entity.
    /// </summary>
    internal static PropertyTypeDto BuildPropertyTypeDto(int groupId, IPropertyType propertyType, int contentTypeId) => new()
    {
        Alias = propertyType.Alias,
        ContentTypeId = contentTypeId,
        DataTypeId = propertyType.DataTypeId,
        Description = propertyType.Description,
        Mandatory = propertyType.Mandatory,
        MandatoryMessage = propertyType.MandatoryMessage,
        Name = propertyType.Name,
        SortOrder = propertyType.SortOrder,
        ValidationRegExp = propertyType.ValidationRegExp,
        ValidationRegExpMessage = propertyType.ValidationRegExpMessage,
        UniqueId = propertyType.Key,
        Variations = (byte)propertyType.Variations,
        LabelOnTop = propertyType.LabelOnTop,
        PropertyTypeGroupId = groupId != default ? groupId : null,
    };
}
