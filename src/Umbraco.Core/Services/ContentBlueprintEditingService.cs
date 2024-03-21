using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

// not implementing ContentEditingServiceWithSortingBase - it might be for later as it has Move, Copy, etc.
// FIXME: Refactor IContentEditingService and IContentBlueprintEditingService - they share logic
internal sealed class ContentBlueprintEditingService : IContentBlueprintEditingService
{
    private readonly IContentService _contentService;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public ContentBlueprintEditingService(
        IContentService contentService,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver)
    {
        _contentService = contentService;
        _scopeProvider = scopeProvider;
        _userIdKeyResolver = userIdKeyResolver;
    }

    public async Task<IContent?> GetAsync(Guid key)
    {
        IContent? blueprint = _contentService.GetBlueprintById(key);
        return await Task.FromResult(blueprint);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        IContent? blueprint = await GetAsync(key);
        if (blueprint == null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, blueprint);
        }

        var performingUserId = await _userIdKeyResolver.GetAsync(userKey);

        _contentService.DeleteBlueprint(blueprint, performingUserId);

        scope.Complete();
        return Attempt.SucceedWithStatus<IContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, blueprint);
    }
}
