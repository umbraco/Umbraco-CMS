using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class ContentCreatingService : IContentCreatingService
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IContentService _contentService;
    private readonly IEntityService _entityService;

    public ContentCreatingService(IContentTypeService contentTypeService, IContentService contentService, IEntityService entityService)
    {
        _contentTypeService = contentTypeService;
        _contentService = contentService;
        _entityService = entityService;
    }

    public async Task<Attempt<PagedModel<IContentType>?, ContentCreatingOperationStatus>> GetAllowedChildrenAsync(Guid key, int skip, int take)
    {
        IContent? content = _contentService.GetById(key);

        if (content is null)
        {
            return Attempt.FailWithStatus<PagedModel<IContentType>?, ContentCreatingOperationStatus>(ContentCreatingOperationStatus.NotFound, null);
        }

        // FIXME: When content gets a ContentTypeKey property, we no longer have to get the key from the entityService
        Attempt<Guid> contentTypeKeyAttempt = _entityService.GetKey(content.ContentTypeId, UmbracoObjectTypes.DocumentType);

        if (contentTypeKeyAttempt.Success is false)
        {
            // This should never happen, content shouldn't be able to exists without a document type
            throw new InvalidOperationException("The document type could not be found.");
        }

        Attempt<PagedModel<IContentType>?, ContentTypeOperationStatus> contentTypeAttempt = await _contentTypeService.GetAllowedChildrenAsync(contentTypeKeyAttempt.Result, skip, take);

        if (contentTypeAttempt.Success is false)
        {
            throw new InvalidOperationException("The document type could not be found.");
        }

        return Attempt.SucceedWithStatus(ContentCreatingOperationStatus.Success, contentTypeAttempt.Result);
    }
}
