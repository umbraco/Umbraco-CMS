using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides API endpoints for managing domains associated with documents.
/// </summary>
[ApiVersion("1.0")]
public class DomainsController : DocumentControllerBase
{
    private readonly IDomainService _domainService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.DomainsController"/> class.
    /// </summary>
    /// <param name="domainService">Service used to manage domain-related operations.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco objects to API models.</param>
    public DomainsController(IDomainService domainService, IUmbracoMapper umbracoMapper)
    {
        _domainService = domainService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves the list of domains and their associated culture settings assigned to the specified document.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document for which to retrieve domain assignments.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="DomainsResponseModel"/> with the assigned domains and culture settings, or a <see cref="ProblemDetails"/> if the document is not found.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/domains")]
    [ProducesResponseType(typeof(DomainsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets domains for a document.")]
    [EndpointDescription("Gets the domains and culture settings assigned to the document identified by the provided Id.")]
    public async Task<IActionResult> Domains(CancellationToken cancellationToken, Guid id)
    {
        IDomain[] assignedDomains = (await _domainService.GetAssignedDomainsAsync(id, true))
            .OrderBy(d => d.SortOrder)
            .ToArray();

        DomainsResponseModel responseModel = _umbracoMapper.Map<DomainsResponseModel>(assignedDomains)!;
        return Ok(responseModel);
    }
}
