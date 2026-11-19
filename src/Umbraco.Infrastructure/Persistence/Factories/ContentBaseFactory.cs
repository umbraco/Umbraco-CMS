using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using EFCoreDtos = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal sealed class ContentBaseFactory
{
    /// <summary>
    ///     Creates a <see cref="Content"/> entity from the EF Core rows a read query returns for one version of a
    ///     document.
    /// </summary>
    /// <remarks>
    ///     Template IDs are NOT set by this method — the repository is responsible for assigning
    ///     <see cref="ITemplatedContent.TemplateId"/> and <see cref="ITemplatedContent.PublishTemplateId"/>
    ///     after calling this method.
    /// </remarks>
    /// <param name="dto">The document row, read for its node id and its published and edited flags.</param>
    /// <param name="node">The node row.</param>
    /// <param name="contentVersion">The content version row of the version being built.</param>
    /// <param name="publishedContentVersion">
    ///     The content version row of the published version, or <c>null</c> if the document has never been published.
    /// </param>
    /// <param name="contentType">The content type; required, a document cannot exist without one.</param>
    /// <returns>A <see cref="Content"/> instance populated from the rows.</returns>
    public static Content BuildEntity(
        EFCoreDtos.DocumentDto dto,
        EFCoreDtos.NodeDto node,
        EFCoreDtos.ContentVersionDto contentVersion,
        EFCoreDtos.ContentVersionDto? publishedContentVersion,
        IContentType contentType)
    {
        var entity = new Content(contentVersion.Text ?? string.Empty, node.ParentId, contentType);

        try
        {
            entity.DisableChangeTracking();
            PopulatePublishableEntity(entity, dto, node, contentVersion, publishedContentVersion);
            entity.ResetDirtyProperties(false);
            return entity;
        }
        finally
        {
            entity.EnableChangeTracking();
        }
    }

    /// <summary>
    ///     Creates an <see cref="IElement" /> entity from the EF Core rows a read query returns for one version of an
    ///     element.
    /// </summary>
    /// <param name="dto">The element row, read for its node id and its published and edited flags.</param>
    /// <param name="node">The node row.</param>
    /// <param name="contentVersion">The content version row of the version being built.</param>
    /// <param name="publishedContentVersion">
    ///     The content version row of the published version, or <c>null</c> if the element has never been published.
    /// </param>
    /// <param name="contentType">The element type; required, an element cannot exist without one.</param>
    /// <returns>An <see cref="Element" /> instance populated from the rows.</returns>
    public static Element BuildEntity(
        EFCoreDtos.ElementDto dto,
        EFCoreDtos.NodeDto node,
        EFCoreDtos.ContentVersionDto contentVersion,
        EFCoreDtos.ContentVersionDto? publishedContentVersion,
        IContentType contentType)
    {
        var entity = new Element(contentVersion.Text ?? string.Empty, node.ParentId, contentType);

        try
        {
            entity.DisableChangeTracking();
            PopulatePublishableEntity(entity, dto, node, contentVersion, publishedContentVersion);
            entity.ResetDirtyProperties(false);
            return entity;
        }
        finally
        {
            entity.EnableChangeTracking();
        }
    }

    /// <summary>
    ///     Constructs a Media item entity from the provided data transfer object (DTO) and media type.
    /// </summary>
    /// <param name="dto">The <see cref="ContentDto"/> containing the data for the media item.</param>
    /// <param name="contentType">The <see cref="IMediaType"/> representing the type of the media item, or <c>null</c> if not specified.</param>
    /// <returns>A <see cref="Media"/> entity built from the specified DTO and media type.</returns>
    public static Core.Models.Media BuildEntity(ContentDto dto, IMediaType? contentType)
    {
        NodeDto nodeDto = dto.NodeDto;
        ContentVersionDto contentVersionDto = dto.ContentVersionDto;

        var content = new Core.Models.Media(
            nodeDto.Text ?? throw new ArgumentException("The media did not have a name", nameof(dto)),
            nodeDto.ParentId,
            contentType);

        try
        {
            content.DisableChangeTracking();

            content.Id = dto.NodeId;
            content.Key = nodeDto.UniqueId;
            content.VersionId = contentVersionDto.Id;

            // TODO: missing names?
            content.Path = nodeDto.Path;
            content.Level = nodeDto.Level;
            content.ParentId = nodeDto.ParentId;
            content.SortOrder = nodeDto.SortOrder;
            content.Trashed = nodeDto.Trashed;

            content.CreatorId = nodeDto.UserId ?? Constants.Security.UnknownUserId;
            content.WriterId = contentVersionDto.UserId ?? Constants.Security.UnknownUserId;
            content.CreateDate = nodeDto.CreateDate.EnsureUtc();
            content.UpdateDate = contentVersionDto.VersionDate.EnsureUtc();

            // reset dirty initial properties (U4-1946)
            content.ResetDirtyProperties(false);
            return content;
        }
        finally
        {
            content.EnableChangeTracking();
        }
    }

    /// <summary>
    /// Builds a <see cref="Member"/> instance from the specified data transfer object and member type.
    /// </summary>
    /// <param name="dto">The <see cref="MemberDto"/> containing the member data.</param>
    /// <param name="contentType">The <see cref="IMemberType"/> describing the member type, or <c>null</c> if not specified.</param>
    /// <returns>The constructed <see cref="Member"/> instance.</returns>
    public static Member BuildEntity(MemberDto dto, IMemberType? contentType)
    {
        NodeDto nodeDto = dto.ContentDto.NodeDto;
        ContentVersionDto contentVersionDto = dto.ContentVersionDto;

        var content = new Member(
            nodeDto.Text ?? throw new ArgumentException("The member did not have a name", nameof(dto)),
            dto.Email,
            dto.LoginName,
            dto.Password,
            contentType);

        try
        {
            content.DisableChangeTracking();

            content.Id = dto.NodeId;
            content.SecurityStamp = dto.SecurityStampToken;
            content.EmailConfirmedDate = dto.EmailConfirmedDate.HasValue
                ? dto.EmailConfirmedDate.Value.EnsureUtc()
                : null;
            content.PasswordConfiguration = dto.PasswordConfig;
            content.Key = nodeDto.UniqueId;
            content.VersionId = contentVersionDto.Id;

            // TODO: missing names?
            content.Path = nodeDto.Path;
            content.Level = nodeDto.Level;
            content.ParentId = nodeDto.ParentId;
            content.SortOrder = nodeDto.SortOrder;
            content.Trashed = nodeDto.Trashed;

            content.CreatorId = nodeDto.UserId ?? Constants.Security.UnknownUserId;
            content.WriterId = contentVersionDto.UserId ?? Constants.Security.UnknownUserId;
            content.CreateDate = nodeDto.CreateDate.EnsureUtc();
            content.UpdateDate = contentVersionDto.VersionDate.EnsureUtc();
            content.FailedPasswordAttempts = dto.FailedPasswordAttempts ?? default;
            content.IsLockedOut = dto.IsLockedOut;
            content.IsApproved = dto.IsApproved;
            content.LastLockoutDate = dto.LastLockoutDate.HasValue
                ? dto.LastLockoutDate.Value.EnsureUtc()
                : null;
            content.LastLoginDate = dto.LastLoginDate.HasValue
                ? dto.LastLoginDate.Value.EnsureUtc()
                : null;
            content.LastPasswordChangeDate = dto.LastPasswordChangeDate.HasValue
                ? dto.LastPasswordChangeDate.Value.EnsureUtc()
                : null;

            // reset dirty initial properties (U4-1946)
            content.ResetDirtyProperties(false);
            return content;
        }
        finally
        {
            content.EnableChangeTracking();
        }
    }

    /// <summary>
    ///     Creates a <see cref="MediaDto"/> instance from the specified <see cref="IMedia"/> entity.
    /// </summary>
    /// <param name="mediaUrlGenerators">A collection of media URL generators used to resolve media URLs.</param>
    /// <param name="entity">The <see cref="IMedia"/> entity to convert to a DTO.</param>
    /// <returns>A <see cref="MediaDto"/> representing the provided media entity.</returns>
    public static MediaDto BuildDto(MediaUrlGeneratorCollection mediaUrlGenerators, IMedia entity)
    {
        ContentDto contentDto = BuildContentDto(entity, Constants.ObjectTypes.Media);

        var dto = new MediaDto
        {
            NodeId = entity.Id,
            ContentDto = contentDto,
            MediaVersionDto = BuildMediaVersionDto(mediaUrlGenerators, entity, contentDto),
        };

        return dto;
    }

    /// <summary>
    ///     Builds a <see cref="MemberDto"/> from the specified <see cref="IMember"/> entity.
    /// </summary>
    /// <param name="entity">The <see cref="IMember"/> entity to build the DTO from.</param>
    /// <returns>A <see cref="MemberDto"/> representing the given <paramref name="entity"/>.</returns>
    public static MemberDto BuildDto(IMember entity)
    {
        ContentDto contentDto = BuildContentDto(entity, Constants.ObjectTypes.Member);

        var dto = new MemberDto
        {
            Email = entity.Email,
            LoginName = entity.Username,
            NodeId = entity.Id,
            Password = entity.RawPasswordValue,
            SecurityStampToken = entity.SecurityStamp,
            EmailConfirmedDate = entity.EmailConfirmedDate,
            ContentDto = contentDto,
            ContentVersionDto = BuildContentVersionDto(entity, contentDto),
            PasswordConfig = entity.PasswordConfiguration,
            FailedPasswordAttempts = entity.FailedPasswordAttempts,
            IsApproved = entity.IsApproved,
            IsLockedOut = entity.IsLockedOut,
            LastLockoutDate = entity.LastLockoutDate,
            LastLoginDate = entity.LastLoginDate,
            LastPasswordChangeDate = entity.LastPasswordChangeDate,
        };
        return dto;
    }

    private static ContentDto BuildContentDto(IContentBase entity, Guid objectType)
    {
        var dto = new ContentDto
        {
            NodeId = entity.Id, ContentTypeId = entity.ContentTypeId, NodeDto = BuildNodeDto(entity, objectType),
        };

        return dto;
    }

    private static NodeDto BuildNodeDto(IContentBase entity, Guid objectType)
    {
        var dto = new NodeDto
        {
            NodeId = entity.Id,
            UniqueId = entity.Key,
            ParentId = entity.ParentId,
            Level = Convert.ToInt16(entity.Level),
            Path = entity.Path,
            SortOrder = entity.SortOrder,
            Trashed = entity.Trashed,
            UserId = entity.CreatorId,
            Text = entity.Name,
            NodeObjectType = objectType,
            CreateDate = entity.CreateDate,
        };

        return dto;
    }

    // always build the current / VersionPk dto
    // we're never going to build / save old versions (which are immutable)
    private static ContentVersionDto BuildContentVersionDto(IContentBase entity, ContentDto contentDto)
    {
        var dto = new ContentVersionDto
        {
            Id = entity.VersionId,
            NodeId = entity.Id,
            VersionDate = entity.UpdateDate,
            UserId = entity.WriterId,
            Current = true, // always building the current one
            Text = entity.Name,
            ContentDto = contentDto,
        };

        return dto;
    }

    /// <summary>
    ///     Builds the EF Core <see cref="EFCoreDtos.DocumentDto"/> row inserted for a new <see cref="IContent"/> entity.
    ///     The node, content and version rows persisted alongside it are built separately.
    /// </summary>
    /// <param name="entity">The content entity, with its id assigned.</param>
    /// <param name="published">The published flag to persist.</param>
    /// <param name="edited">The edited flag to persist.</param>
    /// <returns>An EF Core <see cref="EFCoreDtos.DocumentDto"/> for the specified content entity.</returns>
    internal static EFCoreDtos.DocumentDto BuildEFCoreDocumentDto(IContent entity, bool published, bool edited) =>
        new()
        {
            NodeId = entity.Id,
            Published = published,
            Edited = edited,
        };

    /// <summary>
    ///     Builds the EF Core <see cref="EFCoreDtos.DocumentVersionDto"/> row for the entity's current version.
    /// </summary>
    /// <param name="entity">The content entity whose current version is being persisted.</param>
    /// <param name="publishing">Whether that version is being published as part of the same save.</param>
    /// <returns>An EF Core <see cref="EFCoreDtos.DocumentVersionDto"/> for the entity's current version.</returns>
    internal static EFCoreDtos.DocumentVersionDto BuildEFCoreDocumentVersionDto(IContent entity, bool publishing) =>
        new()
        {
            Id = entity.VersionId,
            TemplateId = entity.TemplateId,
            Published = publishing,
        };

    /// <summary>
    ///     Builds the EF Core <see cref="EFCoreDtos.ElementDto"/> row inserted for a new <see cref="IElement"/> entity.
    ///     The node, content and version rows persisted alongside it are built separately.
    /// </summary>
    /// <param name="entity">The element entity, with its id assigned.</param>
    /// <param name="published">The published flag to persist.</param>
    /// <param name="edited">The edited flag to persist.</param>
    /// <returns>An EF Core <see cref="EFCoreDtos.ElementDto"/> for the specified element entity.</returns>
    internal static EFCoreDtos.ElementDto BuildEFCoreElementDto(IElement entity, bool published, bool edited) =>
        new()
        {
            NodeId = entity.Id,
            Published = published,
            Edited = edited,
        };

    /// <summary>
    ///     Builds the EF Core <see cref="EFCoreDtos.ElementVersionDto"/> row for the entity's current version.
    /// </summary>
    /// <param name="entity">The element entity whose current version is being persisted.</param>
    /// <param name="publishing">Whether that version is being published as part of the same save.</param>
    /// <returns>An EF Core <see cref="EFCoreDtos.ElementVersionDto"/> for the entity's current version.</returns>
    internal static EFCoreDtos.ElementVersionDto BuildEFCoreElementVersionDto(IElement entity, bool publishing) =>
        new()
        {
            Id = entity.VersionId,
            Published = publishing,
        };

    /// <summary>
    ///     Builds the EF Core <see cref="EFCoreDtos.ContentDto"/> row for the specified entity.
    /// </summary>
    internal static EFCoreDtos.ContentDto BuildEFCoreContentDto(IContentBase entity) =>
        new()
        {
            NodeId = entity.Id,
            ContentTypeId = entity.ContentTypeId,
        };

    /// <summary>
    ///     Builds the EF Core <see cref="EFCoreDtos.NodeDto"/> row for the specified entity.
    /// </summary>
    internal static EFCoreDtos.NodeDto BuildEFCoreNodeDto(IContentBase entity, Guid objectType) =>
        new()
        {
            NodeId = entity.Id,
            UniqueId = entity.Key,
            ParentId = entity.ParentId,
            Level = Convert.ToInt16(entity.Level),
            Path = entity.Path,
            SortOrder = entity.SortOrder,
            Trashed = entity.Trashed,
            // EF Core writes the backing field directly, bypassing the UserId getter's 0-to-null coalescing,
            // so an unknown creator must be coalesced here to avoid violating the nodeUser FK.
            UserId = entity.CreatorId == 0 ? null : entity.CreatorId,
            Text = entity.Name,
            NodeObjectType = objectType,
            CreateDate = entity.CreateDate,
        };

    /// <summary>
    ///     Builds the EF Core <see cref="EFCoreDtos.ContentVersionDto"/> row for the entity's current version. Only the
    ///     current version is ever built; older versions are immutable.
    /// </summary>
    /// <param name="entity">The entity whose current version is being persisted.</param>
    /// <param name="publishing">
    ///     Whether that version is being published as part of the same save, in which case it stops being the current
    ///     draft and a new draft takes its place.
    /// </param>
    internal static EFCoreDtos.ContentVersionDto BuildEFCoreContentVersionDto(IContentBase entity, bool publishing) =>
        new()
        {
            Id = entity.VersionId,
            NodeId = entity.Id,
            VersionDate = entity.UpdateDate,
            // See the UserId comment in BuildEFCoreNodeDto - EF Core bypasses the 0-to-null coalescing getter.
            UserId = entity.WriterId == 0 ? null : entity.WriterId,
            Current = !publishing,
            Text = entity.Name,
        };

    private static MediaVersionDto BuildMediaVersionDto(MediaUrlGeneratorCollection mediaUrlGenerators, IMedia entity, ContentDto contentDto)
    {
        // try to get a path from the string being stored for media
        // TODO: only considering umbracoFile
        string? path = null;

        if (entity.Properties.TryGetValue(Constants.Conventions.Media.File, out IProperty? property)
            && mediaUrlGenerators.TryGetMediaPath(property.PropertyType.PropertyEditorAlias, property.GetValue(), out var mediaPath))
        {
            path = mediaPath;
        }

        var dto = new MediaVersionDto
        {
            Id = entity.VersionId, Path = path, ContentVersionDto = BuildContentVersionDto(entity, contentDto),
        };

        return dto;
    }

    /// <summary>
    ///     Populates <paramref name="content"/> from the EF Core rows a read query returns for one version of a
    ///     publishable entity. Does not manage change tracking — the calling <c>BuildEntity</c> method owns that
    ///     lifecycle.
    /// </summary>
    private static void PopulatePublishableEntity<TEntity>(
        TEntity content,
        EFCoreDtos.IPublishableContentDto dto,
        EFCoreDtos.NodeDto node,
        EFCoreDtos.ContentVersionDto contentVersion,
        EFCoreDtos.ContentVersionDto? publishedContentVersion)
        where TEntity : PublishableContentBase
    {
        content.Id = dto.NodeId;
        content.Key = node.UniqueId;
        content.VersionId = contentVersion.Id;
        content.Name = contentVersion.Text;
        content.Path = node.Path;
        content.Level = node.Level;
        content.ParentId = node.ParentId;
        content.SortOrder = node.SortOrder;
        content.Trashed = node.Trashed;
        content.CreatorId = node.UserId ?? Constants.Security.UnknownUserId;
        content.WriterId = contentVersion.UserId ?? Constants.Security.UnknownUserId;
        content.CreateDate = node.CreateDate.EnsureUtc();
        content.UpdateDate = contentVersion.VersionDate.EnsureUtc();
        content.Published = dto.Published;
        content.Edited = dto.Edited;

        if (publishedContentVersion is not null)
        {
            content.PublishedVersionId = publishedContentVersion.Id;
            if (dto.Published)
            {
                content.PublishDate = publishedContentVersion.VersionDate.EnsureUtc();
                content.PublishName = publishedContentVersion.Text;
                content.PublisherId = publishedContentVersion.UserId;
            }
        }
    }
}
