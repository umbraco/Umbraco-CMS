using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.DocumentBlueprint}")]
[ApiExplorerSettings(GroupName = "Document Blueprint")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class DocumentBlueprintTreeControllerBase : FolderTreeControllerBase<DocumentBlueprintTreeItemResponseModel>
{
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public DocumentBlueprintTreeControllerBase(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService)
        => _documentPresentationFactory = documentPresentationFactory;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DocumentBlueprint;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.DocumentBlueprintContainer;

    protected override Ordering ItemOrdering
    {
        get
        {
            var ordering = Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.NodeObjectType), Direction.Descending); // We need to override to change direction
            ordering.Next = Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.Text));

            return ordering;
        }
    }

    protected override DocumentBlueprintTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentId, IEntitySlim[] entities)
        => entities.Select(entity =>
        {
            DocumentBlueprintTreeItemResponseModel responseModel = MapTreeItemViewModel(parentId, entity);
            if (entity is IDocumentEntitySlim documentEntitySlim)
            {
                responseModel.HasChildren = false;
                responseModel.DocumentType = _documentPresentationFactory.CreateDocumentTypeReferenceResponseModel(documentEntitySlim);
            }
            return responseModel;
        }).ToArray();
}
