using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class UpdateDomainsController : DocumentControllerBase
{
    private readonly IDomainService _domainService;
    private readonly IUmbracoMapper _umbracoMapper;

    public UpdateDomainsController(IDomainService domainService, IUmbracoMapper umbracoMapper)
    {
        _domainService = domainService;
        _umbracoMapper = umbracoMapper;
    }

    [MapToApiVersion("1.0")]
    [HttpPut("{id:guid}/domains")]
    public async Task<IActionResult> UpdateDomainsAsync(Guid id, UpdateDomainsRequestModel updateModel)
    {
        DomainsUpdateModel domainsUpdateModel = _umbracoMapper.Map<DomainsUpdateModel>(updateModel)!;

        Attempt<IEnumerable<IDomain>, DomainOperationStatus> result = await _domainService.UpdateDomainsAsync(id, domainsUpdateModel);

        return result.Status switch
        {
            DomainOperationStatus.Success => Ok(),
            DomainOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the domain update operation.")
                .Build()),
            DomainOperationStatus.ContentNotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The targeted content item was not found.")
                    .Build()),
            DomainOperationStatus.LanguageNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid language specified")
                .WithDetail("One or more of the specified language ISO codes could not be found.")
                .Build()),
            DomainOperationStatus.DuplicateDomainName => Conflict(new ProblemDetailsBuilder()
                .WithTitle("Duplicate domain name detected")
                .WithDetail("One or more of the specified domain names were duplicates, possibly of assignments to other content items.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown domain update operation status.")
                .Build()),
        };
    }
}
