using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class UpdatePublicAccessDocumentController : DocumentControllerBase
{
    private readonly IPublicAccessPresentationFactory _publicAccessPresentationFactory;
    private readonly IPublicAccessService _publicAccessService;

    public UpdatePublicAccessDocumentController(IPublicAccessPresentationFactory publicAccessPresentationFactory, IPublicAccessService publicAccessService)
    {
        _publicAccessPresentationFactory = publicAccessPresentationFactory;
        _publicAccessService = publicAccessService;
    }

    [HttpPut("{id:guid}/public-access")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid id, PublicAccessRequestModel requestModel)
    {
        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> publicAccessEntryAttempt = await _publicAccessPresentationFactory.CreatePublicAccessEntry(requestModel, id);
        if (publicAccessEntryAttempt.Success is false)
        {
            return PublicAccessOperationStatusResult(publicAccessEntryAttempt.Status);
        }

        await _publicAccessService.UpdateAsync(publicAccessEntryAttempt.Result!);

        return Ok(publicAccessEntryAttempt.Result);
    }
}
