using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class AllowedParentsMediaTypeController : MediaTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    public AllowedParentsMediaTypeController(IContentTypeService contentTypeService)
    {
        _contentTypeService = contentTypeService;
    }

    [HttpGet("{id:guid}/allowed-parents")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MediaTypeAllowedParentsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AllowedParentsByKey(
        CancellationToken cancellationToken,
        Guid id)
    {
        Attempt<IEnumerable<Guid>?, ContentTypeOperationStatus> attempt = await _contentTypeService.GetAllowedParentsAsync(id, UmbracoObjectTypes.MediaType);
        if (attempt.Success is false)
        {
            return OperationStatusResult(attempt.Status);
        }

        if (attempt.Result == null || attempt.Result.ToArray().IsCollectionEmpty())
        {
            return Ok(new MediaTypeAllowedParentsResponseModel
            {
                AllowedParentsKeys = [],
            });
        }

        var model = new MediaTypeAllowedParentsResponseModel
        {
            AllowedParentsKeys = attempt.Result,
        };

        return Ok(model);
    }
}
