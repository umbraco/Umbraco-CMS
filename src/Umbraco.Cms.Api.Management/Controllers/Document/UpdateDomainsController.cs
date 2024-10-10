using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class UpdateDomainsController : DocumentControllerBase
{
    private readonly IDomainService _domainService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IDomainPresentationFactory _domainPresentationFactory;

    public UpdateDomainsController(IDomainService domainService, IUmbracoMapper umbracoMapper, IDomainPresentationFactory domainPresentationFactory)
    {
        _domainService = domainService;
        _umbracoMapper = umbracoMapper;
        _domainPresentationFactory = domainPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpPut("{id:guid}/domains")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateDomainsRequestModel updateModel)
    {
        DomainsUpdateModel domainsUpdateModel = _umbracoMapper.Map<DomainsUpdateModel>(updateModel)!;

        Attempt<DomainUpdateResult, DomainOperationStatus> result = await _domainService.UpdateDomainsAsync(id, domainsUpdateModel);

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status, problemDetailsBuilder => result.Status switch
            {
                DomainOperationStatus.Success => Ok(),
                DomainOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                    .WithTitle("Cancelled by notification")
                    .WithDetail("A notification handler prevented the domain update operation.")
                    .Build()),
                DomainOperationStatus.ContentNotFound => NotFound(problemDetailsBuilder
                    .WithTitle("The targeted content item was not found.")
                    .Build()),
                DomainOperationStatus.LanguageNotFound => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid language specified")
                    .WithDetail("One or more of the specified language ISO codes could not be found.")
                    .Build()),
                DomainOperationStatus.DuplicateDomainName => Conflict(problemDetailsBuilder
                    .WithTitle("Duplicate domain name detected")
                    .WithDetail("One or more of the specified domain names were duplicates, possibly of assignments to other content items.")
                    .Build()),
                DomainOperationStatus.ConflictingDomainName => Conflict(problemDetailsBuilder
                    .WithTitle("Conflicting domain name detected")
                    .WithDetail("One or more of the specified domain names were conflicting with domain assignments to other content items.")
                    .WithExtension("conflictingDomainNames", _domainPresentationFactory.CreateDomainAssignmentModels(result.Result.ConflictingDomains.EmptyNull()))
                    .Build()),
                DomainOperationStatus.InvalidDomainName => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid domain name detected")
                    .WithDetail("One or more of the specified domain names were invalid.")
                    .Build()),
                _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                    .WithTitle("Unknown domain update operation status.")
                    .Build()),
            });
    }
}
