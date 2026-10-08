using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;

/// <summary>
/// Serves as the base controller for implementing document type tree operations in the management API.
/// Provides common functionality for controllers that expose document type tree structures.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.DocumentType}")]
[ApiExplorerSettings(GroupName = "Document Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class DocumentTypeTreeControllerBase : FolderTreeControllerBase<DocumentTypeTreeItemResponseModel>
{
    private readonly IContentTypeService _contentTypeService;

    public DocumentTypeTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IContentTypeService contentTypeService)
        : base(entityService, flagProviders, entitySearchService, idKeyMap) =>
        _contentTypeService = contentTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DocumentType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.DocumentTypeContainer;

    protected override async Task<DocumentTypeTreeItemResponseModel[]> MapTreeItemViewModelsAsync(Guid? parentKey, IEntitySlim[] entities)
    {
        var contentTypes = (await _contentTypeService
            .GetManyAsync(entities.Select(entity => entity.Id).ToArray()))
            .ToDictionary(contentType => contentType.Id);

        IEnumerable<Task<DocumentTypeTreeItemResponseModel>> tasks = entities.Select(async entity =>
        {
            DocumentTypeTreeItemResponseModel responseModel = await MapTreeItemViewModelAsync(parentKey, entity);
            if (contentTypes.TryGetValue(entity.Id, out IContentType? contentType))
            {
                responseModel.Icon = contentType.Icon ?? responseModel.Icon;
                responseModel.IsElement = contentType.IsElement;
            }

            return responseModel;
        });

        return await Task.WhenAll(tasks);
    }
}
