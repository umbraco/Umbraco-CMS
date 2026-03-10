using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class DomainsController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainService _domainService;
    private readonly IUmbracoMapper _umbracoMapper;

    [ActivatorUtilitiesConstructor]
    public DomainsController(IAuthorizationService authorizationService, IDomainService domainService, IUmbracoMapper umbracoMapper)
    {
        _authorizationService = authorizationService;
        _domainService = domainService;
        _umbracoMapper = umbracoMapper;
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public DomainsController(IDomainService domainService, IUmbracoMapper umbracoMapper)
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>(),
            domainService,
            umbracoMapper)
    {
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/domains")]
    [ProducesResponseType(typeof(DomainsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets domains for a document.")]
    [EndpointDescription("Gets the domains and culture settings assigned to the document identified by the provided Id.")]
    public async Task<IActionResult> Domains(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IDomain[] assignedDomains = (await _domainService.GetAssignedDomainsAsync(id, true))
            .OrderBy(d => d.SortOrder)
            .ToArray();

        DomainsResponseModel responseModel = _umbracoMapper.Map<DomainsResponseModel>(assignedDomains)!;
        return Ok(responseModel);
    }
}
