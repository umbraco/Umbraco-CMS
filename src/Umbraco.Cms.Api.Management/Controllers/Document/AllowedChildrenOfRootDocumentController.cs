using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
public class AllowedChildrenOfRootDocumentController : DocumentControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllowedChildrenOfRootDocumentController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("root/allowed-document-types")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AllowedChildrenOfRoot(int skip = 0, int take = 100)
    {
        PagedModel<IContentType> allowedChildrenOfRoot = await _contentTypeService.GetAllAllowedAsRootAsync(skip, take);

        List<DocumentTypeResponseModel> viewModels = _umbracoMapper.MapEnumerable<IContentType, DocumentTypeResponseModel>(allowedChildrenOfRoot.Items);

        var pagedViewModel = new PagedViewModel<DocumentTypeResponseModel>
        {
            Total = allowedChildrenOfRoot.Total,
            Items = viewModels,
        };

        return await Task.FromResult(Ok(pagedViewModel));
    }
}
