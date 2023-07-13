using Microsoft.AspNetCore.Authorization;
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
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Document}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessForContentTree)]
public abstract class DocumentTreeControllerBase : UserStartNodeTreeControllerBase<DocumentTreeItemResponseModel>
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IContentTypeService _contentTypeService;
    private string? _culture;

    protected DocumentTreeControllerBase(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IContentTypeService contentTypeService)
        : base(entityService, userStartNodeEntitiesService, dataTypeService)
    {
        _publicAccessService = publicAccessService;
        _appCaches = appCaches;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _contentTypeService = contentTypeService;
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

            responseModel.Variants = MapVariants(documentEntitySlim);

            // TODO: This make this either be part of the IDocumentEntitySlim, or at the very least be more performantly fetched.
            // This sucks, since it'll cost an extra DB call
            // but currently there's no really good way to get the content type key from an IDocumentEntitySlim
            // We have the same issue in DocumentPresentationFactory
            IContentType? contentType = _contentTypeService.Get(documentEntitySlim.ContentTypeAlias);
            responseModel.ContentTypeId = contentType?.Key ?? Guid.Empty;
        }

        return responseModel;
    }

    private IEnumerable<VariantTreeItemViewModel> MapVariants(IDocumentEntitySlim entity)
    {
        if (entity.Variations.VariesByCulture() is false)
        {
            yield return new VariantTreeItemViewModel
            {
                Name = entity.Name ?? string.Empty,
                State = entity.Published ? PublishedState.Published : PublishedState.Unpublished,
                Culture = null,
            };
            yield break;
        }

        foreach (KeyValuePair<string, string> cultureNamePair in entity.CultureNames)
        {
            yield return new VariantTreeItemViewModel
            {
                Name = cultureNamePair.Value,
                Culture = cultureNamePair.Key,
                State = entity.PublishedCultures.Contains(cultureNamePair.Key)
                    ? PublishedState.Published
                    : PublishedState.Unpublished,
            };
        }
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
