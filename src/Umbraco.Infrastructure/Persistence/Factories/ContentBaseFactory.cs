using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal sealed class ContentBaseFactory
{
    /// <summary>
    ///     Creates a <see cref="Content"/> entity from the specified <see cref="DocumentDto"/> and <see cref="IContentType"/>.
    /// </summary>
    /// <param name="dto">The <see cref="DocumentDto"/> containing the data used to construct the content entity.</param>
    /// <param name="contentType">The <see cref="IContentType"/> that defines the structure and properties of the content.</param>
    /// <returns>A <see cref="Content"/> instance populated with data from the provided <paramref name="dto"/> and <paramref name="contentType"/>.</returns>
    public static Content BuildEntity(DocumentDto dto, IContentType? contentType)
    {
        ContentDto contentDto = dto.ContentDto;
        NodeDto nodeDto = contentDto.NodeDto;
        DocumentVersionDto documentVersionDto = dto.DocumentVersionDto;
        ContentVersionDto contentVersionDto = documentVersionDto.ContentVersionDto;
        DocumentVersionDto? publishedVersionDto = dto.PublishedVersionDto;

        var content = new Content(nodeDto.Text, nodeDto.ParentId, contentType);

        try
        {
            content.DisableChangeTracking();

            content.Id = dto.NodeId;
            content.Key = nodeDto.UniqueId;
            content.VersionId = contentVersionDto.Id;

            content.Name = contentVersionDto.Text;

            content.Path = nodeDto.Path;
            content.Level = nodeDto.Level;
            content.ParentId = nodeDto.ParentId;
            content.SortOrder = nodeDto.SortOrder;
            content.Trashed = nodeDto.Trashed;

            content.CreatorId = nodeDto.UserId ?? Constants.Security.UnknownUserId;
            content.WriterId = contentVersionDto.UserId ?? Constants.Security.UnknownUserId;

            content.CreateDate = nodeDto.CreateDate.EnsureUtc();
            content.UpdateDate = contentVersionDto.VersionDate.EnsureUtc();

            content.Published = dto.Published;
            content.Edited = dto.Edited;

            if (publishedVersionDto != null)
            {
                // We need this to get the proper versionId to match to unpublished values.
                // This is only needed if the content has been published before.
                content.PublishedVersionId = publishedVersionDto.Id;
                if (dto.Published)
                {
                    content.PublishDate = publishedVersionDto.ContentVersionDto.VersionDate.EnsureUtc();
                    content.PublishName = publishedVersionDto.ContentVersionDto.Text;
                    content.PublisherId = publishedVersionDto.ContentVersionDto.UserId;
                }
            }

            // templates = ignored, managed by the repository

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
    ///     Constructs a Media item entity from the provided data transfer object (DTO) and media type.
    /// </summary>
    /// <param name="dto">The <see cref="ContentDto"/> containing the data for the media item.</param>
    /// <param name="contentType">The <see cref="IMediaType"/> representing the type of the media item, or <c>null</c> if not specified.</param>
    /// <returns>A <see cref="Media"/> entity built from the specified DTO and media type.</returns>
    public static Core.Models.Media BuildEntity(ContentDto dto, IMediaType? contentType)
    {
        NodeDto nodeDto = dto.NodeDto;
        ContentVersionDto contentVersionDto = dto.ContentVersionDto;

        var content = new Core.Models.Media(nodeDto.Text, nodeDto.ParentId, contentType);

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

        var content = new Member(nodeDto.Text, dto.Email, dto.LoginName, dto.Password, contentType);

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
    /// Creates a <see cref="DocumentDto"/> instance from the specified <see cref="IContent"/> entity and object type identifier.
    /// </summary>
    /// <param name="entity">The content entity to convert into a DTO.</param>
    /// <param name="objectType">The unique identifier representing the object type.</param>
    /// <returns>A <see cref="DocumentDto"/> representing the specified content entity.</returns>
    public static DocumentDto BuildDto(IContent entity, Guid objectType)
    {
        ContentDto contentDto = BuildContentDto(entity, objectType);

        var dto = new DocumentDto
        {
            NodeId = entity.Id,
            Published = entity.Published,
            ContentDto = contentDto,
            DocumentVersionDto = BuildDocumentVersionDto(entity, contentDto),
        };

        return dto;
    }

    /// <summary>
    /// Creates a collection of tuples pairing each <see cref="ContentSchedule"/> in the provided schedule with its corresponding <see cref="ContentScheduleDto"/>.
    /// </summary>
    /// <param name="entity">The content entity associated with the schedules.</param>
    /// <param name="contentSchedule">The collection of content schedules to convert.</param>
    /// <param name="languageRepository">The repository used to resolve language identifiers for each schedule.</param>
    /// <returns>An enumerable of tuples, each containing a <see cref="ContentSchedule"/> and its corresponding <see cref="ContentScheduleDto"/>.</returns>
    public static IEnumerable<(ContentSchedule Model, ContentScheduleDto Dto)> BuildScheduleDto(
        IContent entity,
        ContentScheduleCollection contentSchedule,
        ILanguageRepository languageRepository) =>
        contentSchedule.FullSchedule.Select(x =>
            (x,
                new ContentScheduleDto
                {
                    Action = x.Action.ToString(),
                    Date = x.Date,
                    NodeId = entity.Id,
                    LanguageId = languageRepository.GetIdByIsoCode(x.Culture, false),
                    Id = x.Id,
                }));

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

    // always build the current / VersionPk dto
    // we're never going to build / save old versions (which are immutable)
    private static DocumentVersionDto BuildDocumentVersionDto(IContent entity, ContentDto contentDto)
    {
        var dto = new DocumentVersionDto
        {
            Id = entity.VersionId,
            TemplateId = entity.TemplateId,
            Published = false, // always building the current, unpublished one

            ContentVersionDto = BuildContentVersionDto(entity, contentDto),
        };

        return dto;
    }

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
}
