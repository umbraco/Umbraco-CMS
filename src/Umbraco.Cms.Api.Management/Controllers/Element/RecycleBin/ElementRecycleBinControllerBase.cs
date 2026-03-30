using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Element.Folder;
using Umbraco.Cms.Api.Management.Controllers.RecycleBin;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Element.RecycleBin;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

/// <summary>
/// Serves as the base controller for element recycle bin operations within the Umbraco CMS Management API.
/// Provides shared functionality for derived element recycle bin controllers.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.RecycleBin}/{Constants.UdiEntityType.Element}")]
[RequireElementTreeRootAccess]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessElements)]
public class ElementRecycleBinControllerBase : RecycleBinControllerBase<ElementRecycleBinItemResponseModel>
{
    /// <inheritdoc />
    protected override string EntityName => "element";

    private readonly IEntityService _entityService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementRecycleBinControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public ElementRecycleBinControllerBase( IEntityService entityService, IElementPresentationFactory elementPresentationFactory)
        : base(entityService)
    {
        _entityService = entityService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Element;

    protected override Guid RecycleBinRootKey => Constants.System.RecycleBinElementKey;

    protected override ElementRecycleBinItemResponseModel MapRecycleBinViewModel(Guid? parentId, IEntitySlim entity)
    {
        ElementRecycleBinItemResponseModel responseModel = base.MapRecycleBinViewModel(parentId, entity);

        responseModel.Name = entity.Name ?? string.Empty;

        if (entity is IElementEntitySlim elementEntitySlim)
        {
            responseModel.Variants = _elementPresentationFactory.CreateVariantsItemResponseModels(elementEntitySlim);
            responseModel.DocumentType = _elementPresentationFactory.CreateDocumentTypeReferenceResponseModel(elementEntitySlim);
            responseModel.IsFolder = false;
        }
        else
        {
            responseModel.Variants = [];
            responseModel.IsFolder = true;
        }

        return responseModel;
    }

    protected override IEntitySlim[] GetPagedRootEntities(int skip, int take, out long totalItems)
        => GetPagedChildEntities(RecycleBinRootKey, skip, take, out totalItems);

    protected override IEntitySlim[] GetPagedChildEntities(Guid parentKey, int skip, int take, out long totalItems)
    {
        IEntitySlim[] rootEntities = _entityService
            .GetPagedChildren(
                parentKey,
                parentObjectTypes: [UmbracoObjectTypes.ElementContainer],
                childObjectTypes: [UmbracoObjectTypes.ElementContainer, ItemObjectType],
                skip: skip,
                take: take,
                trashed: true,
                out totalItems)
            .ToArray();

        return rootEntities;
    }

    protected override IEntitySlim[] GetSiblingEntities(Guid target, int before, int after, out long totalBefore, out long totalAfter) =>
        _entityService
            .GetTrashedSiblings(
                target,
                objectTypes: [UmbracoObjectTypes.ElementContainer, ItemObjectType],
                before,
                after,
                out totalBefore,
                out totalAfter,
                ordering: Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.Text)))
            .ToArray();

    protected IActionResult OperationStatusResult(EntityContainerOperationStatus status)
        => ElementFolderControllerBase.OperationStatusResult(status);
}
