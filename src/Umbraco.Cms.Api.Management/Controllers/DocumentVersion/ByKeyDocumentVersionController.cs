using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

[ApiVersion("1.0")]
public class ByKeyDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IContentVersionService _contentVersionService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyDocumentVersionController(
        IContentVersionService contentVersionService,
        IUmbracoMapper umbracoMapper)
    {
        _contentVersionService = contentVersionService;
        _umbracoMapper = umbracoMapper;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentVersionResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IContent?, ContentVersionOperationStatus> attempt =
            await _contentVersionService.GetAsync(id);

        return attempt.Success
            ? Ok(_umbracoMapper.Map<DocumentVersionResponseModel>(attempt.Result))
            : MapFailure(attempt.Status);
    }
}
