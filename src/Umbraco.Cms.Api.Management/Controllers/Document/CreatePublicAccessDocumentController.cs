using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class CreatePublicAccessDocumentController : DocumentControllerBase
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly IPublicAccessPresentationFactory _publicAccessPresentationFactory;

    public CreatePublicAccessDocumentController(
        IPublicAccessService publicAccessService,
        IPublicAccessPresentationFactory publicAccessPresentationFactory)
    {
        _publicAccessService = publicAccessService;
        _publicAccessPresentationFactory = publicAccessPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{id:guid}/public-access")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid id, PublicAccessRequestModel publicAccessRequestModel)
    {
        PublicAccessEntrySlim publicAccessEntrySlim = _publicAccessPresentationFactory.CreatePublicAccessEntrySlim(publicAccessRequestModel, id);

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> saveAttempt = await _publicAccessService.CreateAsync(publicAccessEntrySlim);

        return saveAttempt.Success
            ? CreatedAtAction<GetPublicAccessDocumentController>(controller => nameof(controller.GetPublicAccess), saveAttempt.Result!.Key)
            : PublicAccessOperationStatusResult(saveAttempt.Status);
    }
}
