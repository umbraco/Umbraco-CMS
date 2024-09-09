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

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class AllowedAtRootDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllowedAtRootDocumentTypeController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("allowed-at-root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AllowedDocumentType>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AllowedAtRoot(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        PagedModel<IContentType> result = await _contentTypeService.GetAllAllowedAsRootAsync(skip, take);

        List<AllowedDocumentType> viewModels = _umbracoMapper.MapEnumerable<IContentType, AllowedDocumentType>(result.Items);

        var pagedViewModel = new PagedViewModel<AllowedDocumentType>
        {
            Total = result.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
