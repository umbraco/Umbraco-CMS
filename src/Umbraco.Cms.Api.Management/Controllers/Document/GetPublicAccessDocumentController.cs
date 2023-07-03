using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class GetPublicAccessDocumentController : DocumentControllerBase
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly IPublicAccessPresentationFactory _publicAccessPresentationFactory;

    public GetPublicAccessDocumentController(IPublicAccessService publicAccessService, IPublicAccessPresentationFactory publicAccessPresentationFactory)
    {
        _publicAccessService = publicAccessService;
        _publicAccessPresentationFactory = publicAccessPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/public-access")]
    public async Task<IActionResult> GetPublicAccess(Guid id)
    {
        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> accessAttempt = await _publicAccessService.GetEntryByContentKeyAsync(id);

        if (accessAttempt.Success is false)
        {
            return PublicAccessOperationStatusResult(accessAttempt.Status);
        }

        Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus> responseModelAttempt = await _publicAccessPresentationFactory.CreatePublicAccessResponseModel(accessAttempt.Result!);

        if (responseModelAttempt.Success is false)
        {
            return PublicAccessOperationStatusResult(responseModelAttempt.Status);
        }

        return Ok(responseModelAttempt.Result);
    }
}
