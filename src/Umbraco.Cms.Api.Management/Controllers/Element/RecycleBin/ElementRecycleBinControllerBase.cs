using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.RecycleBin;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Element.RecycleBin;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.RecycleBin}/{Constants.UdiEntityType.Element}")]
// TODO ELEMENTS: backoffice authorization policies
[RequireDocumentTreeRootAccess]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
// TODO ELEMENTS: backoffice authorization policies
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
public class ElementRecycleBinControllerBase : RecycleBinControllerBase<ElementRecycleBinItemResponseModel>
{
    private readonly IElementPresentationFactory _elementPresentationFactory;

    public ElementRecycleBinControllerBase( IEntityService entityService, IElementPresentationFactory elementPresentationFactory)
        : base(entityService)
        => _elementPresentationFactory = elementPresentationFactory;

    // TODO ELEMENTS: recycle bin controller base must support multiple item object types to handle both containers and elements
    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Element;

    protected override Guid RecycleBinRootKey => Constants.System.RecycleBinElementKey;

    protected override ElementRecycleBinItemResponseModel MapRecycleBinViewModel(Guid? parentId, IEntitySlim entity)
    {
        ElementRecycleBinItemResponseModel responseModel = base.MapRecycleBinViewModel(parentId, entity);

        if (entity is IElementEntitySlim elementEntitySlim)
        {
            responseModel.Variants = _elementPresentationFactory.CreateVariantsItemResponseModels(elementEntitySlim);
            responseModel.DocumentType = _elementPresentationFactory.CreateDocumentTypeReferenceResponseModel(elementEntitySlim);
        }

        return responseModel;
    }
}
