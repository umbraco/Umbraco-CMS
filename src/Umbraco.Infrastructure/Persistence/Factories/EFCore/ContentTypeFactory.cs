using System.Globalization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories.EFCore;

// EF Core factory for IContentType (document types), IMediaType (media types) and IMemberType (member types).
internal static class ContentTypeFactory
{
    /// <summary>
    /// Creates and initializes an <see cref="IContentType"/> entity from the specified <see cref="ContentTypeDto"/>.
    /// </summary>
    public static IContentType BuildContentTypeEntity(IShortStringHelper shortStringHelper, ContentTypeDto dto)
    {
        var contentType = new ContentType(shortStringHelper, dto.NodeDto.ParentId);
        try
        {
            contentType.DisableChangeTracking();
            BuildCommonEntity(contentType, dto);
            contentType.ResetDirtyProperties(false);
            return contentType;
        }
        finally
        {
            contentType.EnableChangeTracking();
        }
    }

    /// <summary>
    /// Creates and initializes an <see cref="IMediaType"/> entity from the specified <see cref="ContentTypeDto"/>.
    /// </summary>
    public static IMediaType BuildMediaTypeEntity(IShortStringHelper shortStringHelper, ContentTypeDto dto)
    {
        var contentType = new MediaType(shortStringHelper, dto.NodeDto.ParentId);
        try
        {
            contentType.DisableChangeTracking();
            BuildCommonEntity(contentType, dto);
            contentType.ResetDirtyProperties(false);
        }
        finally
        {
            contentType.EnableChangeTracking();
        }

        return contentType;
    }

    /// <summary>
    /// Creates and initializes an <see cref="IMemberType"/> entity from the specified <see cref="ContentTypeDto"/>.
    /// </summary>
    public static IMemberType BuildMemberTypeEntity(IShortStringHelper shortStringHelper, ContentTypeDto dto)
    {
        var contentType = new MemberType(shortStringHelper, dto.NodeDto.ParentId);
        try
        {
            contentType.DisableChangeTracking();
            BuildCommonEntity(contentType, dto, setVariations: false);
            contentType.ResetDirtyProperties(false);
        }
        finally
        {
            contentType.EnableChangeTracking();
        }

        return contentType;
    }

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
        entity.AllowedInLibrary = dto.AllowedInLibrary;
        entity.Trashed = dto.NodeDto.Trashed;

        if (setVariations)
        {
            entity.Variations = (ContentVariation)dto.Variations;
        }
    }

    /// <summary>
    /// Creates a <see cref="ContentTypeDto"/> (including its nested <see cref="NodeDto"/>) from the specified
    /// <see cref="IContentTypeBase"/> entity. Determines the appropriate node object type based on the entity's
    /// concrete type (content, media, or member type).
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

        return new ContentTypeDto
        {
            Alias = entity.Alias,
            Description = entity.Description,
            Icon = entity.Icon,
            Thumbnail = entity.Thumbnail,
            NodeId = entity.Id,
            AllowAtRoot = entity.AllowedAsRoot,
            ListView = entity.ListView,
            IsElement = entity.IsElement,
            AllowedInLibrary = entity.AllowedInLibrary,
            Variations = (byte)entity.Variations,
            NodeDto = BuildNodeDto(entity, nodeObjectType),
        };
    }

    private static NodeDto BuildNodeDto(IUmbracoEntity entity, Guid nodeObjectType) => new()
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
        // EF Core writes the backing field directly, bypassing the UserId getter's 0-to-null coalescing,
        // so an unknown creator must be coalesced here to avoid violating the nodeUser FK.
        UserId = entity.CreatorId == 0 ? null : entity.CreatorId,
    };
}
