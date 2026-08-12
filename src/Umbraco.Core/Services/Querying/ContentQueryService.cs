using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentQuery;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Querying;

/// <summary>
/// Implements the content query service.
/// </summary>
public class ContentQueryService : IContentQueryService
{
    private readonly IContentService _contentService;
    private readonly ICoreScopeProvider _coreScopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentQueryService"/> class.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="coreScopeProvider">The core scope provider.</param>
    public ContentQueryService(
        IContentService contentService,
        ICoreScopeProvider coreScopeProvider)
    {
        _contentService = contentService;
        _coreScopeProvider = coreScopeProvider;
    }

    /// <summary>
    /// Gets the content with schedules.
    /// </summary>
    /// <param name="id">The id of the content.</param>
    /// <returns>The content with schedules.</returns>
    public async Task<Attempt<ContentScheduleQueryResult?, ContentQueryOperationStatus>> GetWithSchedulesAsync(Guid id)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);

        IContent? content = await _contentService.GetByIdAsync(id, CancellationToken.None);

        if (content == null)
        {
            return Attempt<ContentScheduleQueryResult, ContentQueryOperationStatus>.Fail(ContentQueryOperationStatus
                .ContentNotFound);
        }

        ContentScheduleCollection schedules = _contentService.GetContentScheduleByContentId(id);

        return Attempt<ContentScheduleQueryResult?, ContentQueryOperationStatus>
            .Succeed(
                ContentQueryOperationStatus.Success,
                new ContentScheduleQueryResult(content, schedules));
    }
}
