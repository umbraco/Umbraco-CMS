using System.Globalization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories.EFCore;

// EF Core factory for IContentType (document types), IMediaType (media types) and IMemberType (member types).
internal static class ContentTypeFactory
{
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
