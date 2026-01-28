using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Element}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessForElementTree)]
public class ElementTreeControllerBase : UserStartNodeFolderTreeControllerBase<ElementTreeItemResponseModel>
{
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    public ElementTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementPresentationFactory elementPresentationFactory)
        : base(entityService, flagProviders, userStartNodeEntitiesService, dataTypeService)
    {
        _appCaches = appCaches;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _elementPresentationFactory = elementPresentationFactory;
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Element;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.ElementContainer;

    protected override int[] GetUserStartNodeIds()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .CalculateElementStartNodeIds(EntityService, _appCaches)
           ?? [];

    protected override string[] GetUserStartNodePaths()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .GetElementStartNodePaths(EntityService, _appCaches)
           ?? [];

    protected override ElementTreeItemResponseModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        ElementTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentKey, entity);

        if (entity is IElementEntitySlim elementEntitySlim)
        {
            responseModel.HasChildren = false;
            responseModel.CreateDate = elementEntitySlim.CreateDate;
            responseModel.DocumentType = _elementPresentationFactory.CreateDocumentTypeReferenceResponseModel(elementEntitySlim);
            responseModel.Variants = _elementPresentationFactory.CreateVariantsItemResponseModels(elementEntitySlim);
        }

        return responseModel;
    }

    protected override ElementTreeItemResponseModel MapTreeItemViewModelAsNoAccess(Guid? parentKey, IEntitySlim entity)
    {
        ElementTreeItemResponseModel viewModel = MapTreeItemViewModel(parentKey, entity);
        viewModel.NoAccess = true;
        return viewModel;
    }
}
