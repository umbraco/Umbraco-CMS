﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class DomainsController : DocumentControllerBase
{
    private readonly IDomainService _domainService;
    private readonly IUmbracoMapper _umbracoMapper;

    public DomainsController(IDomainService domainService, IUmbracoMapper umbracoMapper)
    {
        _domainService = domainService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{key:guid}/domains")]
    public async Task<IActionResult> DomainsAsync(Guid key)
    {
        IDomain[] assignedDomains = (await _domainService.GetAssignedDomainsAsync(key, true))
            .OrderBy(d => d.SortOrder)
            .ToArray();

        DomainsResponseModel responseModel = _umbracoMapper.Map<DomainsResponseModel>(assignedDomains)!;
        return Ok(responseModel);
    }
}
