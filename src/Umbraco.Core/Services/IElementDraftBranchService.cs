using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Manages named draft branches for <see cref="IElement"/> elements — independently editable, in-progress
/// drafts that exist alongside an element's default draft, so multiple future states of the same element
/// can be prepared in parallel. Mirrors <see cref="IContentDraftBranchService"/> for documents.
/// </summary>
public interface IElementDraftBranchService
{
    /// <inheritdoc cref="IContentDraftBranchService.CreateAsync" />
    Task<Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus>> CreateAsync(Guid contentKey, string name, Guid userKey);

    /// <inheritdoc cref="IContentDraftBranchService.GetByKey" />
    IContentDraftBranch? GetByKey(Guid contentKey, Guid branchKey);

    /// <inheritdoc cref="IContentDraftBranchService.GetByContentKey" />
    IEnumerable<IContentDraftBranch> GetByContentKey(Guid contentKey);

    /// <inheritdoc cref="IContentDraftBranchService.GetContent" />
    IElement? GetContent(Guid contentKey, Guid branchKey);

    /// <inheritdoc cref="IContentDraftBranchService.SaveAsync" />
    Task<Attempt<ContentDraftBranchOperationStatus>> SaveAsync(Guid contentKey, Guid branchKey, IElement content, Guid userKey);

    /// <inheritdoc cref="IContentDraftBranchService.Abandon" />
    Attempt<ContentDraftBranchOperationStatus> Abandon(Guid contentKey, Guid branchKey);

    /// <inheritdoc cref="IContentDraftBranchService.PromoteAsync" />
    Task<Attempt<ContentDraftBranchOperationStatus>> PromoteAsync(Guid contentKey, Guid branchKey, ISet<string> cultures, Guid userKey);

    /// <inheritdoc cref="IContentDraftBranchService.GetVersions" />
    IEnumerable<IElement> GetVersions(Guid contentKey, Guid? branchKey = null);

    /// <inheritdoc cref="IContentDraftBranchService.GetVersionsSlim" />
    IEnumerable<IElement> GetVersionsSlim(Guid contentKey, int skip, int take, Guid? branchKey = null);

    /// <inheritdoc cref="IContentDraftBranchService.Schedule" />
    Attempt<ContentDraftBranchOperationStatus> Schedule(Guid contentKey, Guid branchKey, DateTime? publishDate, ISet<string> cultures);
}
