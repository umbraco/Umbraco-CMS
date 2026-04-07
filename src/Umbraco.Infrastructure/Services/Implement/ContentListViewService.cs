using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal sealed class ContentListViewService : ContentListViewServiceBase<IContent, IContentType, IContentTypeService>, IContentListViewService
{
    private readonly IContentService _contentService;
    private readonly IContentPermissionAuthorizer _contentPermissionAuthorizer;

    protected override Guid DefaultListViewKey => Constants.DataTypes.Guids.ListViewContentGuid;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentListViewService"/> class.
    /// </summary>
    /// <param name="contentService">Service used for managing and accessing content items.</param>
    /// <param name="contentTypeService">Service used for managing content types and their metadata.</param>
    /// <param name="dataTypeService">Service used for managing data types associated with content properties.</param>
    /// <param name="contentSearchService">Service used for searching and querying content items.</param>
    /// <param name="contentPermissionAuthorizer">Service responsible for authorizing content permissions and access control.</param>
    public ContentListViewService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IContentSearchService contentSearchService,
        IContentPermissionAuthorizer contentPermissionAuthorizer)
        : base(contentTypeService, dataTypeService, contentSearchService)
    {
        _contentService = contentService;
        _contentPermissionAuthorizer = contentPermissionAuthorizer;
    }

    /// <summary>
    /// Asynchronously retrieves a paged list view model of content items for the specified content item key.
    /// </summary>
    /// <param name="user">The user requesting the content list view items.</param>
    /// <param name="key">The unique key identifying the content item whose list view items are to be retrieved.</param>
    /// <param name="dataTypeKey">An optional key specifying the data type to filter the list view items.</param>
    /// <param name="orderBy">The name of the field by which to order the results.</param>
    /// <param name="orderCulture">An optional culture identifier to use for ordering localized fields.</param>
    /// <param name="orderDirection">The direction in which to order the results (ascending or descending).</param>
    /// <param name="filter">An optional filter string to apply to the list view items.</param>
    /// <param name="skip">The number of items to skip (for paging).</param>
    /// <param name="take">The number of items to return (for paging).</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an <see cref="Attempt{T, TStatus}"/> with a paged list view model of content items if successful, or a failure status otherwise.
    /// </returns>
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

    // We can use an authorizer here, as it already handles all the necessary checks for this filtering.
    // However, we cannot pass in all the items; we want only the ones that comply, as opposed to
    // a general response whether the user has access to all nodes.
    [Obsolete("This is no longer used as we now authorize collection view items as a collection via FilterAuthorizedKeysAsync rather than one by one. Scheduled for removal in Umbraco 19.")]
    protected override async Task<bool> HasAccessToListViewItemAsync(IUser user, Guid key)
    {
        var isDenied = await _contentPermissionAuthorizer.IsDeniedAsync(
            user,
            key,
            ActionBrowse.ActionLetter);

        return isDenied is false;
    }

    /// <inheritdoc/>
    protected override async Task<ISet<Guid>> FilterAuthorizedKeysAsync(IUser user, IEnumerable<Guid> keys) =>
        await _contentPermissionAuthorizer.FilterAuthorizedAsync(
            user,
            keys,
            new HashSet<string> { ActionBrowse.ActionLetter });
}
