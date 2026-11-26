using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class ContentDraftBranchFactory
{
    /// <summary>
    /// Creates an <see cref="IContentDraftBranch"/> entity from the specified <see cref="ContentDraftBranchDto"/>.
    /// </summary>
    public static IContentDraftBranch BuildEntity(ContentDraftBranchDto dto)
    {
        var branch = new ContentDraftBranch(dto.NodeId, dto.ContentKey, dto.Name, dto.BasedOnVersionId, dto.CreatedByUserId)
        {
            Id = dto.Id,
            Key = dto.Key,
            CreateDate = dto.CreateDate,
            UpdateDate = dto.UpdateDate,
            ScheduledPublishDate = dto.ScheduledPublishDate,
            ScheduledCultures = dto.ScheduledCultures?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet() ?? new HashSet<string>(),
        };

        branch.ResetDirtyProperties(false);

        return branch;
    }

    /// <summary>
    /// Builds a <see cref="ContentDraftBranchDto"/> from the given <see cref="IContentDraftBranch"/> entity.
    /// </summary>
    public static ContentDraftBranchDto BuildDto(IContentDraftBranch entity)
    {
        var dto = new ContentDraftBranchDto
        {
            Id = entity.Id,
            Key = entity.Key,
            NodeId = entity.NodeId,
            ContentKey = entity.ContentKey,
            Name = entity.Name,
            CreatedByUserId = entity.CreatedByUserId,
            CreateDate = entity.CreateDate,
            UpdateDate = entity.UpdateDate,
            BasedOnVersionId = entity.BasedOnVersionId,
            ScheduledPublishDate = entity.ScheduledPublishDate,

            // kept in lock-step with ScheduledPublishDate: null only when no schedule has ever been set,
            // otherwise the joined culture set (which may itself be an empty string for invariant content).
            ScheduledCultures = entity.ScheduledPublishDate is null ? null : string.Join(',', entity.ScheduledCultures),
        };

        return dto;
    }
}
