using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal sealed class ContentListViewService : ContentListViewServiceBase<IContent, IContentType, IContentTypeService>, IContentListViewService
{
    private readonly IContentService _contentService;
    private readonly IContentPermissionService _contentPermissionService;

    public ContentListViewService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        IContentPermissionService contentPermissionService,
        IDataTypeService dataTypeService,
        ISqlContext sqlContext)
        : base(contentTypeService, dataTypeService, sqlContext)
    {
        _contentService = contentService;
        _contentPermissionService = contentPermissionService;
    }

    protected override Guid DefaultListViewKey => Constants.DataTypes.Guids.ListViewContentGuid;

    protected override IEnumerable<IContent> GetPagedChildren(
        int id,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter,
        Ordering? ordering) =>
        _contentService.GetPagedChildren(
            id,
            pageIndex,
            pageSize,
            out totalRecords,
            filter,
            ordering);

    // We can use an authorizer here, as it already handles all the necessary checks for this filtering.
    // However, we cannot pass in all the items; we want only the ones that comply, as opposed to
    // a general response whether the user has access to all nodes.
    protected override async Task<bool> HasAccessToListViewItemAsync(IUser user, Guid key)
    {
        // TODO: Consider if it is better to use IContentPermissionAuthorizer here as people will be able to apply their external authorization
        ContentAuthorizationStatus accessStatus = await _contentPermissionService.AuthorizeAccessAsync(
            user,
            key,
            ActionBrowse.ActionLetter);

        // var isAuthorized = await _contentPermissionAuthorizer.IsAuthorizedAsync(
        //     user, //IPrincipal
        //     item.Key,
        //     ActionBrowse.ActionLetter);
        //
        // return isAuthorized;

        return accessStatus == ContentAuthorizationStatus.Success;
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
}
