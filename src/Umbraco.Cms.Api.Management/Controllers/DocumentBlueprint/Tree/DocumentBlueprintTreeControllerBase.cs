﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.DocumentBlueprint}")]
[ApiExplorerSettings(GroupName = "Document Blueprint")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public class DocumentBlueprintTreeControllerBase : NamedEntityTreeControllerBase<DocumentBlueprintTreeItemResponseModel>
{
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public DocumentBlueprintTreeControllerBase(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService)
        => _documentPresentationFactory = documentPresentationFactory;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DocumentBlueprint;

    protected override DocumentBlueprintTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentId, IEntitySlim[] entities)
    {
        IDocumentEntitySlim[] documentEntities = entities
            .OfType<IDocumentEntitySlim>()
            .ToArray();

        return documentEntities.Select(entity =>
        {
            DocumentBlueprintTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentId, entity);
            responseModel.HasChildren = false;
            responseModel.DocumentType = _documentPresentationFactory.CreateDocumentTypeReferenceResponseModel(entity);
            return responseModel;
        }).ToArray();
    }
}
