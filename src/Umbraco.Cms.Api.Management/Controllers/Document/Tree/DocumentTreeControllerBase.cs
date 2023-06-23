using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Extensions;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Document}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
public abstract class DocumentTreeControllerBase : UserStartNodeTreeControllerBase<DocumentTreeItemResponseModel>
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private string? _culture;

    protected DocumentTreeControllerBase(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, userStartNodeEntitiesService, dataTypeService)
    {
        _publicAccessService = publicAccessService;
        _appCaches = appCaches;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override Ordering ItemOrdering => Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.SortOrder));

    protected void RenderForClientCulture(string? culture) => _culture = culture;

    protected override DocumentTreeItemResponseModel MapTreeItemViewModel(Guid? parentId, IEntitySlim entity)
    {
        DocumentTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentId, entity);

        if (entity is IDocumentEntitySlim documentEntitySlim)
        {
            responseModel.IsPublished = documentEntitySlim.Published;
            responseModel.IsEdited = documentEntitySlim.Edited;
            responseModel.Icon = documentEntitySlim.ContentTypeIcon ?? responseModel.Icon;
            responseModel.IsProtected = _publicAccessService.IsProtected(entity.Path);
            responseModel.IsTrashed = entity.Trashed;
            responseModel.Id = entity.Key;

            if (_culture != null && documentEntitySlim.Variations.VariesByCulture())
            {
                responseModel.Name = documentEntitySlim.CultureNames.TryGetValue(_culture, out var cultureName)
                    ? cultureName
                    : $"({responseModel.Name})";

                responseModel.IsPublished = documentEntitySlim.PublishedCultures.Contains(_culture);
                responseModel.IsEdited = documentEntitySlim.EditedCultures.Contains(_culture);
            }

            responseModel.IsEdited &= responseModel.IsPublished;
        }

        return responseModel;
    }

    // TODO: delete these (faking start node setup for unlimited editor)
    protected override int[] GetUserStartNodeIds() => new[] { -1 };

    protected override string[] GetUserStartNodePaths() => Array.Empty<string>();

    // TODO: use these implementations instead of the dummy ones above once we have backoffice auth in place
    // protected override int[] GetUserStartNodeIds()
    //     => _backofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .CalculateContentStartNodeIds(EntityService, _appCaches)
    //        ?? Array.Empty<int>();
    //
    // protected override string[] GetUserStartNodePaths()
    //     => _backofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .GetContentStartNodePaths(EntityService, _appCaches)
    //        ?? Array.Empty<string>();
}
