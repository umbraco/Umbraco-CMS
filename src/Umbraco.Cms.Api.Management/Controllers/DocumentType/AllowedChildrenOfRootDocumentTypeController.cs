using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

public class AllowedChildrenOfRootDocumentTypeController : ManagementApiControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllowedChildrenOfRootDocumentTypeController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("allowed-children-of/root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AllowedChildrenOfRoot(int skip = 0, int take = 100)
    {
        IEnumerable<IContentType> allowedChildrenOfRoot = _contentTypeService.GetAll().Where(x => x.AllowedAsRoot).ToArray();

        List<DocumentTypeResponseModel> viewModels = _umbracoMapper.MapEnumerable<IContentType, DocumentTypeResponseModel>(allowedChildrenOfRoot.Skip(skip).Take(take));

        var pagedViewModel = new PagedViewModel<DocumentTypeResponseModel>()
        {
            Total = allowedChildrenOfRoot.Count(),
            Items = viewModels,
        };

        return await Task.FromResult(Ok(pagedViewModel));
    }
}
