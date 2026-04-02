using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTypeTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service used for managing and retrieving entities within the Umbraco CMS.</param>
    /// <param name="contentTypeService">Service used for managing and retrieving content types (document types) within the Umbraco CMS.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public DocumentTypeTreeControllerBase(IEntityService entityService, IContentTypeService contentTypeService)
        : this(
              entityService,
              StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>(),
              contentTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTypeTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations such as retrieval and management.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document types.</param>
    /// <param name="contentTypeService">Service responsible for managing content types.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public DocumentTypeTreeControllerBase(IEntityService entityService, FlagProviderCollection flagProviders, IContentTypeService contentTypeService)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IEntitySearchService>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>(),
            contentTypeService)
    {
    }

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

    protected override DocumentTypeTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        var contentTypes = _contentTypeService
            .GetMany(entities.Select(entity => entity.Id).ToArray())
            .ToDictionary(contentType => contentType.Id);

        return entities.Select(entity =>
        {
            DocumentTypeTreeItemResponseModel responseModel = MapTreeItemViewModel(parentKey, entity);
            if (contentTypes.TryGetValue(entity.Id, out IContentType? contentType))
            {
                responseModel.Icon = contentType.Icon ?? responseModel.Icon;
                responseModel.IsElement = contentType.IsElement;
            }

            return responseModel;
        }).ToArray();
    }
}
