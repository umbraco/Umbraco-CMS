using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.RecycleBin;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Document.RecycleBin;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.RecycleBin}/{Constants.UdiEntityType.Document}")]
[RequireDocumentTreeRootAccess]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
public class DocumentRecycleBinControllerBase : RecycleBinControllerBase<DocumentRecycleBinItemResponseModel>
{
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public DocumentRecycleBinControllerBase(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService)
        => _documentPresentationFactory = documentPresentationFactory;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override Guid RecycleBinRootKey => Constants.System.RecycleBinContentKey;

    protected override DocumentRecycleBinItemResponseModel MapRecycleBinViewModel(Guid? parentId, IEntitySlim entity)
    {
        DocumentRecycleBinItemResponseModel responseModel = base.MapRecycleBinViewModel(parentId, entity);

        if (entity is IDocumentEntitySlim documentEntitySlim)
        {
            responseModel.Variants = _documentPresentationFactory.CreateVariantsItemResponseModels(documentEntitySlim);
            responseModel.DocumentType = _documentPresentationFactory.CreateDocumentTypeReferenceResponseModel(documentEntitySlim);
        }

        return responseModel;
    }
}
