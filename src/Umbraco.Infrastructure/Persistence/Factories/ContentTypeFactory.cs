using System.Globalization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

// factory for
// IContentType (document types)
// IMediaType (media types)
// IMemberType (member types)
//
internal static class ContentTypeFactory
{
    #region IContentType

    /// <summary>
    /// Creates and initializes an <see cref="IContentType"/> entity from the specified <see cref="ContentTypeDto"/>.
    /// This includes setting common entity properties and resetting initial property states.
    /// </summary>
    /// <param name="shortStringHelper">The helper used for string manipulation and formatting.</param>
    /// <param name="dto">The data transfer object containing the content type's data.</param>
    /// <returns>A fully constructed <see cref="IContentType"/> entity with change tracking re-enabled after initialization.</returns>
    public static IContentType BuildContentTypeEntity(IShortStringHelper shortStringHelper, ContentTypeDto dto)
    {
        var contentType = new ContentType(shortStringHelper, dto.NodeDto.ParentId);

        try
        {
            contentType.DisableChangeTracking();

            BuildCommonEntity(contentType, dto);

            // reset dirty initial properties (U4-1946)
            contentType.ResetDirtyProperties(false);
            return contentType;
        }
        finally
        {
            contentType.EnableChangeTracking();
        }
    }

    #endregion

    #region IMediaType

    public static IMediaType BuildMediaTypeEntity(IShortStringHelper shortStringHelper, ContentTypeDto dto)
    {
        var contentType = new MediaType(shortStringHelper, dto.NodeDto.ParentId);
        try
        {
            contentType.DisableChangeTracking();

            BuildCommonEntity(contentType, dto);

            // reset dirty initial properties (U4-1946)
            contentType.ResetDirtyProperties(false);
        }
        finally
        {
            contentType.EnableChangeTracking();
        }

        return contentType;
    }

    #endregion

    #region IMemberType

    /// <summary>
    /// Constructs an <see cref="Umbraco.Cms.Core.Models.IMemberType"/> entity from the specified <see cref="ContentTypeDto"/>,
    /// initializing it with data from the DTO and using the provided <see cref="IShortStringHelper"/> for string operations.
    /// </summary>
    /// <param name="shortStringHelper">An instance of <see cref="IShortStringHelper"/> used to assist with string manipulation during entity creation.</param>
    /// <param name="dto">A <see cref="ContentTypeDto"/> containing the data required to build the member type entity.</param>
    /// <returns>
    /// A new <see cref="Umbraco.Cms.Core.Models.IMemberType"/> instance populated with values from the provided DTO.
    /// </returns>
    public static IMemberType BuildMemberTypeEntity(IShortStringHelper shortStringHelper, ContentTypeDto dto)
    {
        var contentType = new MemberType(shortStringHelper, dto.NodeDto.ParentId);
        try
        {
            contentType.DisableChangeTracking();
            BuildCommonEntity(contentType, dto, false);
            contentType.ResetDirtyProperties(false);
        }
        finally
        {
            contentType.EnableChangeTracking();
        }

        return contentType;
    }

    /// <summary>
    /// Creates a collection of <see cref="Umbraco.Cms.Core.Models.Membership.MemberPropertyTypeDto"/> objects representing the property types defined on the specified <see cref="Umbraco.Cms.Core.Models.IMemberType"/>.
    /// </summary>
    /// <param name="entity">The member type entity whose property types will be converted to DTOs.</param>
    /// <returns>An enumerable of <see cref="Umbraco.Cms.Core.Models.Membership.MemberPropertyTypeDto"/> for each property type on the member type, or an empty collection if none exist.</returns>
    public static IEnumerable<MemberPropertyTypeDto> BuildMemberPropertyTypeDtos(IMemberType entity)
    {
        if (entity is not MemberType memberType || memberType.PropertyTypes.Any() == false)
        {
            return Enumerable.Empty<MemberPropertyTypeDto>();
        }

        var dtos = memberType.PropertyTypes.Select(x => new MemberPropertyTypeDto
        {
            NodeId = entity.Id,
            PropertyTypeId = x.Id,
            CanEdit = memberType.MemberCanEditProperty(x.Alias),
            ViewOnProfile = memberType.MemberCanViewProperty(x.Alias),
            IsSensitive = memberType.IsSensitiveProperty(x.Alias),
        }).ToList();
        return dtos;
    }

    /// <summary>
    /// Creates a <see cref="ContentTypeDto"/> instance from the specified <see cref="IContentTypeBase"/> entity.
    /// Determines the appropriate node object type based on the entity's concrete type (content, media, or member type).
    /// </summary>
    /// <param name="entity">The <see cref="IContentTypeBase"/> entity to convert to a DTO.</param>
    /// <returns>A <see cref="ContentTypeDto"/> that represents the provided content type entity.</returns>
    public static ContentTypeDto BuildContentTypeDto(IContentTypeBase entity)
    {
        Guid nodeObjectType;
        if (entity is IContentType)
        {
            nodeObjectType = Constants.ObjectTypes.DocumentType;
        }
        else if (entity is IMediaType)
        {
            nodeObjectType = Constants.ObjectTypes.MediaType;
        }
        else if (entity is IMemberType)
        {
            nodeObjectType = Constants.ObjectTypes.MemberType;
        }
        else
        {
            throw new Exception("Invalid entity.");
        }

        var contentTypeDto = new ContentTypeDto
        {
            Alias = entity.Alias,
            Description = entity.Description,
            Icon = entity.Icon,
            Thumbnail = entity.Thumbnail,
            NodeId = entity.Id,
            AllowAtRoot = entity.AllowedAsRoot,
            ListView = entity.ListView,
            IsElement = entity.IsElement,
            Variations = (byte)entity.Variations,
            NodeDto = BuildNodeDto(entity, nodeObjectType),
        };
        return contentTypeDto;
    }

    #endregion

    #region Common

    private static void BuildCommonEntity(ContentTypeBase entity, ContentTypeDto dto, bool setVariations = true)
    {
        entity.Id = dto.NodeDto.NodeId;
        entity.Key = dto.NodeDto.UniqueId;
        entity.Alias = dto.Alias ?? string.Empty;
        entity.Name = dto.NodeDto.Text;
        entity.Icon = dto.Icon;
        entity.Thumbnail = dto.Thumbnail;
        entity.SortOrder = dto.NodeDto.SortOrder;
        entity.Description = dto.Description;
        entity.CreateDate = dto.NodeDto.CreateDate.EnsureUtc();
        entity.UpdateDate = dto.NodeDto.CreateDate.EnsureUtc();
        entity.Path = dto.NodeDto.Path;
        entity.Level = dto.NodeDto.Level;
        entity.CreatorId = dto.NodeDto.UserId ?? Constants.Security.UnknownUserId;
        entity.AllowedAsRoot = dto.AllowAtRoot;
        entity.ListView = dto.ListView;
        entity.IsElement = dto.IsElement;
        entity.Trashed = dto.NodeDto.Trashed;

        if (setVariations)
        {
            entity.Variations = (ContentVariation)dto.Variations;
        }
    }

    private static NodeDto BuildNodeDto(IUmbracoEntity entity, Guid nodeObjectType)
    {
        var nodeDto = new NodeDto
        {
            CreateDate = entity.CreateDate,
            NodeId = entity.Id,
            Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
            NodeObjectType = nodeObjectType,
            ParentId = entity.ParentId,
            Path = entity.Path,
            SortOrder = entity.SortOrder,
            Text = entity.Name,
            Trashed = false,
            UniqueId = entity.Key,
            UserId = entity.CreatorId,
        };
        return nodeDto;
    }

    #endregion
}
