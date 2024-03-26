using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Versions;

[ApiVersion("1.0")]
public class GetByKeyDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;

    public GetByKeyDocumentVersionController(
        IContentVersionService contentVersionService,
        IUmbracoMapper umbracoMapper)
        : base(contentVersionService)
    {
        _umbracoMapper = umbracoMapper;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentVersionResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(Guid id)
    {
        Attempt<IContent?, ContentVersionOperationStatus> attempt =
            await ContentVersionService.GetAsync(id);

        return attempt.Success is true
            ? Ok(_umbracoMapper.Map<DocumentVersionResponseModel>(attempt.Result))
            : MapFailure(attempt.Status);
    }
}
