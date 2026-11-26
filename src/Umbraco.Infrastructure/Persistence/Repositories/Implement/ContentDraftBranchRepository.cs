using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class ContentDraftBranchRepository : IContentDraftBranchRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public ContentDraftBranchRepository(IScopeAccessor scopeAccessor)
        => _scopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));

    private IScope AmbientScope => _scopeAccessor.AmbientScope
        ?? throw new InvalidOperationException("No ambient scope available for content draft branch repository operations.");

    public IContentDraftBranch? Get(int id)
    {
        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Select<ContentDraftBranchDto>()
            .From<ContentDraftBranchDto>()
            .Where<ContentDraftBranchDto>(x => x.Id == id);

        ContentDraftBranchDto? dto = AmbientScope.Database.FirstOrDefault<ContentDraftBranchDto>(sql);
        return dto is null ? null : ContentDraftBranchFactory.BuildEntity(dto);
    }

    public IContentDraftBranch? Get(Guid key)
    {
        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Select<ContentDraftBranchDto>()
            .From<ContentDraftBranchDto>()
            .Where<ContentDraftBranchDto>(x => x.Key == key);

        ContentDraftBranchDto? dto = AmbientScope.Database.FirstOrDefault<ContentDraftBranchDto>(sql);
        return dto is null ? null : ContentDraftBranchFactory.BuildEntity(dto);
    }

    public IEnumerable<IContentDraftBranch> GetByNodeId(int nodeId)
    {
        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Select<ContentDraftBranchDto>()
            .From<ContentDraftBranchDto>()
            .Where<ContentDraftBranchDto>(x => x.NodeId == nodeId)
            .OrderBy<ContentDraftBranchDto>(x => x.CreateDate);

        return AmbientScope.Database.Fetch<ContentDraftBranchDto>(sql).Select(ContentDraftBranchFactory.BuildEntity);
    }

    public void Save(IContentDraftBranch branch)
    {
        var isNew = !branch.HasIdentity;
        if (isNew)
        {
            branch.AddingEntity();
        }
        else
        {
            branch.UpdatingEntity();
        }

        ContentDraftBranchDto dto = ContentDraftBranchFactory.BuildDto(branch);

        if (isNew)
        {
            dto.Key = branch.Key; // EntityBase.Key lazily generates a Guid on first access; ensure it's assigned before insert.
            var id = Convert.ToInt32(AmbientScope.Database.Insert(dto));
            branch.Id = id;
        }
        else
        {
            AmbientScope.Database.Update(dto);
        }

        branch.ResetDirtyProperties();
    }

    public void Delete(int id)
        => DeleteByIds([id]);

    public IEnumerable<IContentDraftBranch> GetScheduledForPublish(DateTime date)
    {
        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Select<ContentDraftBranchDto>()
            .From<ContentDraftBranchDto>()
            .Where<ContentDraftBranchDto>(x => x.ScheduledPublishDate != null && x.ScheduledPublishDate <= date);

        return AmbientScope.Database.Fetch<ContentDraftBranchDto>(sql).Select(ContentDraftBranchFactory.BuildEntity);
    }

    public void DeleteByIds(IEnumerable<int> ids)
    {
        var idList = ids as IReadOnlyCollection<int> ?? ids.ToList();
        if (idList.Count == 0)
        {
            return;
        }

        // Release the FK first: a branch's own version row keeps pointing at it via BranchId even after
        // MarkBranchVersionSuperseded flips Current to false - that method only retires the row from being
        // "current", it doesn't (and shouldn't, on its own) sever the branch link. Clearing BranchId here
        // just demotes that row to ordinary, unbranched history - it's already non-current - which is exactly
        // what a branch's history becomes once the branch itself is gone.
        AmbientScope.Database.Execute(AmbientScope.SqlContext.Sql()
            .Update<ContentVersionDto>(u => u.Set(x => x.BranchId, null))
            .WhereIn<ContentVersionDto>(x => x.BranchId, idList));

        AmbientScope.Database.Execute(AmbientScope.SqlContext.Sql()
            .Delete<ContentDraftBranchDto>()
            .WhereIn<ContentDraftBranchDto>(x => x.Id, idList));
    }
}
