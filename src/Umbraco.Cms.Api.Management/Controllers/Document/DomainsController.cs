using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class DomainsController : DocumentControllerBase
{
    private readonly IDomainService _domainService;
    private readonly IUmbracoMapper _umbracoMapper;

    public DomainsController(IDomainService domainService, IUmbracoMapper umbracoMapper)
    {
        _domainService = domainService;
        _umbracoMapper = umbracoMapper;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/domains")]
    [ProducesResponseType(typeof(DomainsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Domains(CancellationToken cancellationToken, Guid id)
    {
        IDomain[] assignedDomains = (await _domainService.GetAssignedDomainsAsync(id, true))
            .OrderBy(d => d.SortOrder)
            .ToArray();

        DomainsResponseModel responseModel = _umbracoMapper.Map<DomainsResponseModel>(assignedDomains)!;
        return Ok(responseModel);
    }
}
