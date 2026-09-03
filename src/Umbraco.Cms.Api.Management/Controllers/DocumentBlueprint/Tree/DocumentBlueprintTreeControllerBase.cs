using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

/// <summary>
/// Serves as the base controller for handling operations related to document blueprint trees in the management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.DocumentBlueprint}")]
[ApiExplorerSettings(GroupName = "Document Blueprint")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessForDocumentBlueprintTree)]
public class DocumentBlueprintTreeControllerBase : UserStartNodeFolderTreeControllerBase<DocumentBlueprintTreeItemResponseModel>
{
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentBlueprintTreeControllerBase"/> class, providing required services for managing document blueprint trees.
    /// </summary>
    /// <param name="entityService">The service used to interact with entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    /// <param name="documentPresentationFactory">The factory responsible for creating document presentation models.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public DocumentBlueprintTreeControllerBase(IEntityService entityService, FlagProviderCollection flagProviders, IDocumentPresentationFactory documentPresentationFactory)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IEntitySearchService>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>(),
            documentPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentBlueprintStartNodeTreeFilterService>())
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 21.")]
    public DocumentBlueprintTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IDocumentPresentationFactory documentPresentationFactory)
        : this(
            entityService,
            flagProviders,
            entitySearchService,
            idKeyMap,
            documentPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentBlueprintStartNodeTreeFilterService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentBlueprintTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">The service used to interact with entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    /// <param name="entitySearchService">The service used to search for entities.</param>
    /// <param name="idKeyMap">The map between entity ids and keys.</param>
    /// <param name="documentPresentationFactory">The factory responsible for creating document presentation models.</param>
    /// <param name="treeFilterService">The service used to filter the tree by the user's start nodes.</param>
    [ActivatorUtilitiesConstructor]
    public DocumentBlueprintTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IDocumentPresentationFactory documentPresentationFactory,
        IDocumentBlueprintStartNodeTreeFilterService treeFilterService)
        : base(entityService, flagProviders, entitySearchService, idKeyMap, treeFilterService)
        => _documentPresentationFactory = documentPresentationFactory;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DocumentBlueprint;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.DocumentBlueprintContainer;

    protected override Ordering ItemOrdering
    {
        get
        {
            var ordering = Ordering.By(Infrastructure.Persistence.Dtos.NodeDto.NodeObjectTypeColumnName, Direction.Descending); // We need to override to change direction
            ordering.Next = Ordering.By(Infrastructure.Persistence.Dtos.NodeDto.TextColumnName);

            return ordering;
        }
    }

    /// <inheritdoc />
    protected override async Task<DocumentBlueprintTreeItemResponseModel> MapTreeItemViewModelAsync(Guid? parentKey, IEntitySlim entity)
    {
        DocumentBlueprintTreeItemResponseModel responseModel = await base.MapTreeItemViewModelAsync(parentKey, entity);

        // Containers are read alongside the blueprints, and a query covering blueprints yields every
        // row as a document, so the entity type alone does not tell the two apart.
        if (responseModel.IsFolder is false && entity is IDocumentEntitySlim documentEntitySlim)
        {
            responseModel.HasChildren = false;
            responseModel.DocumentType = _documentPresentationFactory.CreateDocumentTypeReferenceResponseModel(documentEntitySlim);
        }

        return responseModel;
    }

    /// <inheritdoc />
    protected override async Task<DocumentBlueprintTreeItemResponseModel> MapTreeItemViewModelAsNoAccessAsync(Guid? parentKey, IEntitySlim entity)
    {
        DocumentBlueprintTreeItemResponseModel responseModel = await MapTreeItemViewModelAsync(parentKey, entity);
        responseModel.NoAccess = true;
        return responseModel;
    }
}
