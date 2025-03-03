using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal sealed class ContentListViewService : ContentListViewServiceBase<IContent, IContentType, IContentTypeService>, IContentListViewService
{
    private readonly IContentService _contentService;
    private readonly IContentPermissionAuthorizer _contentPermissionAuthorizer;

    protected override Guid DefaultListViewKey => Constants.DataTypes.Guids.ListViewContentGuid;

    public ContentListViewService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        ISqlContext sqlContext,
        IContentPermissionAuthorizer contentPermissionAuthorizer)
        : base(contentTypeService, dataTypeService, sqlContext)
    {
        _contentService = contentService;
        _contentPermissionAuthorizer = contentPermissionAuthorizer;
    }

    public async Task<Attempt<ListViewPagedModel<IContent>?, ContentCollectionOperationStatus>> GetListViewItemsByKeyAsync(
        IUser user,
        Guid key,
        Guid? dataTypeKey,
        string orderBy,
        string? orderCulture,
        Direction orderDirection,
        string? filter,
        int skip,
        int take)
    {
        IContent? content = _contentService.GetById(key);
        if (content == null)
        {
            return Attempt.FailWithStatus<ListViewPagedModel<IContent>?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentNotFound, null);
        }

        return await GetListViewResultAsync(user, content, dataTypeKey, orderBy, orderCulture, orderDirection, filter, skip, take);
    }

    protected override Task<PagedModel<IContent>> GetPagedChildrenAsync(
        int id,
        IQuery<IContent>? filter,
        Ordering? ordering,
        int skip,
        int take)
    {
        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        IEnumerable<IContent> items = _contentService.GetPagedChildren(
            id,
            pageNumber,
            pageSize,
            out var total,
            filter,
            ordering);

        var pagedResult = new PagedModel<IContent>
        {
            Items = items,
            Total = total,
        };

        return Task.FromResult(pagedResult);
    }

    // We can use an authorizer here, as it already handles all the necessary checks for this filtering.
    // However, we cannot pass in all the items; we want only the ones that comply, as opposed to
    // a general response whether the user has access to all nodes.
    protected override async Task<bool> HasAccessToListViewItemAsync(IUser user, Guid key)
    {
        var isDenied = await _contentPermissionAuthorizer.IsDeniedAsync(
            user,
            key,
            ActionBrowse.ActionLetter);

        return isDenied is false;
    }
}
