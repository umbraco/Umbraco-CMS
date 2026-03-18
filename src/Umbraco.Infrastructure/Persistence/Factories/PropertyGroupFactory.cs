using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class PropertyGroupFactory
{
    internal static PropertyTypeGroupDto BuildGroupDto(PropertyGroup propertyGroup, int contentTypeId)
    {
        var dto = new PropertyTypeGroupDto
        {
            UniqueId = propertyGroup.Key,
            Type = (short)propertyGroup.Type,
            ContentTypeNodeId = contentTypeId,
            Text = propertyGroup.Name,
            Alias = propertyGroup.Alias,
            SortOrder = propertyGroup.SortOrder,
        };

        if (propertyGroup.HasIdentity)
        {
            dto.Id = propertyGroup.Id;
        }

        dto.PropertyTypeDtos = propertyGroup.PropertyTypes
            ?.Select(propertyType => BuildPropertyTypeDto(propertyGroup.Id, propertyType, contentTypeId)).ToList()
            ?? [];

        return dto;
    }

    internal static PropertyTypeDto BuildPropertyTypeDto(int groupId, IPropertyType propertyType, int contentTypeId)
    {
        var propertyTypeDto = new PropertyTypeDto
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
        };

        if (groupId != default)
        {
            propertyTypeDto.PropertyTypeGroupId = groupId;
        }
        else
        {
            propertyTypeDto.PropertyTypeGroupId = null;
        }

        if (propertyType.HasIdentity)
        {
            propertyTypeDto.Id = propertyType.Id;
        }

        return propertyTypeDto;
    }

    #region Implementation of IEntityFactory<IEnumerable<PropertyGroup>,IEnumerable<TabDto>>

    /// <summary>
    /// Constructs a collection of <see cref="Umbraco.Cms.Core.Models.PropertyGroup"/> entities from the provided group DTOs, including their associated property types.
    /// </summary>
    /// <param name="groupDtos">A collection of <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.PropertyTypeGroupDto"/> objects representing property groups and their property types.</param>
    /// <param name="isPublishing">Indicates whether the property groups are being built for publishing scenarios.</param>
    /// <param name="contentTypeId">The identifier of the content type that owns the property groups.</param>
    /// <param name="createDate">The creation date to assign to each property type within the groups.</param>
    /// <param name="updateDate">The update date to assign to each property type within the groups.</param>
    /// <param name="propertyTypeCtor">A factory function used to construct <see cref="Umbraco.Cms.Core.Models.PropertyType"/> instances, given the editor alias, value storage type, and property type alias.</param>
    /// <returns>A collection of <see cref="Umbraco.Cms.Core.Models.PropertyGroup"/> entities, each populated with their corresponding property types.</returns>
    public static IEnumerable<PropertyGroup> BuildEntity(
        IEnumerable<PropertyTypeGroupDto> groupDtos,
        bool isPublishing,
        int contentTypeId,
        DateTime createDate,
        DateTime updateDate,
        Func<string?, ValueStorageType, string?, PropertyType> propertyTypeCtor)
    {
        // groupDtos contains all the groups, those that are defined on the current
        // content type, and those that are inherited from composition content types
        var propertyGroups = new PropertyGroupCollection();
        foreach (PropertyTypeGroupDto groupDto in groupDtos)
        {
            var group = new PropertyGroup(isPublishing);

            try
            {
                group.DisableChangeTracking();

                // if the group is defined on the current content type,
                // assign its identifier, else it will be zero
                if (groupDto.ContentTypeNodeId == contentTypeId)
                {
                    group.Id = groupDto.Id;
                }

                group.Key = groupDto.UniqueId;
                group.Type = (PropertyGroupType)groupDto.Type;
                group.Name = groupDto.Text;
                group.Alias = groupDto.Alias;
                group.SortOrder = groupDto.SortOrder;

                group.PropertyTypes = new PropertyTypeCollection(isPublishing);

                // Because we are likely to have a group with no PropertyTypes we need to ensure that these are excluded
                IEnumerable<PropertyTypeDto> typeDtos = groupDto.PropertyTypeDtos?.Where(x => x.Id > 0) ??
                                                        Enumerable.Empty<PropertyTypeDto>();
                foreach (PropertyTypeDto typeDto in typeDtos)
                {
                    PropertyTypeGroupDto tempGroupDto = groupDto;
                    PropertyType propertyType = propertyTypeCtor(
                        typeDto.DataTypeDto.EditorAlias,
                        typeDto.DataTypeDto.DbType.EnumParse<ValueStorageType>(true),
                        typeDto.Alias);

                    try
                    {
                        propertyType.DisableChangeTracking();

                        propertyType.Alias = typeDto.Alias ?? string.Empty;
                        propertyType.DataTypeId = typeDto.DataTypeId;
                        propertyType.DataTypeKey = typeDto.DataTypeDto.NodeDto.UniqueId;
                        propertyType.Description = typeDto.Description;
                        propertyType.Id = typeDto.Id;
                        propertyType.Key = typeDto.UniqueId;
                        propertyType.Name = typeDto.Name ?? string.Empty;
                        propertyType.Mandatory = typeDto.Mandatory;
                        propertyType.MandatoryMessage = typeDto.MandatoryMessage;
                        propertyType.SortOrder = typeDto.SortOrder;
                        propertyType.ValidationRegExp = typeDto.ValidationRegExp;
                        propertyType.ValidationRegExpMessage = typeDto.ValidationRegExpMessage;
                        propertyType.PropertyGroupId = new Lazy<int>(() => tempGroupDto.Id);
                        propertyType.CreateDate = createDate.EnsureUtc();
                        propertyType.UpdateDate = updateDate.EnsureUtc();
                        propertyType.Variations = (ContentVariation)typeDto.Variations;

                        // reset dirty initial properties (U4-1946)
                        propertyType.ResetDirtyProperties(false);
                        group.PropertyTypes.Add(propertyType);
                    }
                    finally
                    {
                        propertyType.EnableChangeTracking();
                    }
                }

                // reset dirty initial properties (U4-1946)
                group.ResetDirtyProperties(false);
                propertyGroups.Add(group);
            }
            finally
            {
                group.EnableChangeTracking();
            }
        }

        return propertyGroups;
    }

    /// <summary>
    /// Converts a collection of <see cref="PropertyGroup"/> entities into a collection of <see cref="PropertyTypeGroupDto"/> objects.
    /// </summary>
    /// <param name="entity">The collection of <see cref="PropertyGroup"/> entities to convert.</param>
    /// <returns>An <see cref="IEnumerable{PropertyTypeGroupDto}"/> representing the converted property groups.</returns>
    public static IEnumerable<PropertyTypeGroupDto> BuildDto(IEnumerable<PropertyGroup> entity) =>
        entity.Select(BuildGroupDto).ToList();

    #endregion
}
