using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class CreatePublicAccessDocumentController : DocumentControllerBase
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly IUmbracoMapper _mapper;

    public CreatePublicAccessDocumentController(IPublicAccessService publicAccessService, IUmbracoMapper mapper)
    {
        _publicAccessService = publicAccessService;
        _mapper = mapper;
    }

    [MapToApiVersion("1.0")]
    [HttpPost("public-access")]
    public async Task<IActionResult> CreatePublicAccess(PublicAccessRequestModel publicAccessRequestModel)
    {
        PublicAccessEntrySlim publicAccessEntrySlim = _mapper.Map<PublicAccessEntrySlim>(publicAccessRequestModel)!;

        Attempt<PublicAccessOperationStatus> saveAttempt = await _publicAccessService.SaveAsync(publicAccessEntrySlim);

        return saveAttempt.Success ? Ok() : PublicAccessOperationStatusResult(saveAttempt.Result);
    }
}
