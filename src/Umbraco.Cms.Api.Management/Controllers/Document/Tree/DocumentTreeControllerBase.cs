using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Document}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessForContentTree)]
public abstract class DocumentTreeControllerBase : UserStartNodeTreeControllerBase<DocumentTreeItemResponseModel>
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    protected DocumentTreeControllerBase(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IDocumentPresentationFactory documentPresentationFactory)
    : base(entityService, userStartNodeEntitiesService, dataTypeService)
    {
        _publicAccessService = publicAccessService;
        _appCaches = appCaches;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _documentPresentationFactory = documentPresentationFactory;
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override Ordering ItemOrdering => Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.SortOrder));

    protected override DocumentTreeItemResponseModel MapTreeItemViewModel(Guid? parentId, IEntitySlim entity)
    {
        DocumentTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentId, entity);

        if (entity is IDocumentEntitySlim documentEntitySlim)
        {
            responseModel.IsProtected = _publicAccessService.IsProtected(entity.Path);
            responseModel.AncestorIds = GetAncestorIds(EntityService.GetPathKeys(entity));
            responseModel.IsTrashed = entity.Trashed;
            responseModel.Id = entity.Key;
            responseModel.CreateDate = entity.CreateDate;

            responseModel.Variants = _documentPresentationFactory.CreateVariantsItemResponseModels(documentEntitySlim);
            responseModel.DocumentType = _documentPresentationFactory.CreateDocumentTypeReferenceResponseModel(documentEntitySlim);
        }

        return responseModel;
    }

    private Guid[] GetAncestorIds(Guid[] keys) =>
        // Omit the last path key as that will be for the item itself.
        keys.Take(keys.Length - 1).ToArray();

    protected override int[] GetUserStartNodeIds()
        => _backofficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .CalculateContentStartNodeIds(EntityService, _appCaches)
           ?? Array.Empty<int>();

    protected override string[] GetUserStartNodePaths()
        => _backofficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .GetContentStartNodePaths(EntityService, _appCaches)
           ?? Array.Empty<string>();
}
