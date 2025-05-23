using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentQuery;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Querying;

public interface IContentQueryService
{
    Task<Attempt<ContentScheduleQueryResult?, ContentQueryOperationStatus>> GetWithSchedulesAsync(Guid id);
}

public class ContentQueryService : IContentQueryService
{
    private readonly IContentService _contentService;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public ContentQueryService(
        IContentService contentService,
        ICoreScopeProvider coreScopeProvider)
    {
        _contentService = contentService;
        _coreScopeProvider = coreScopeProvider;
    }

    public async Task<Attempt<ContentScheduleQueryResult?, ContentQueryOperationStatus>> GetWithSchedulesAsync(Guid id)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);

        IContent? content = await Task.FromResult(_contentService.GetById(id));

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
