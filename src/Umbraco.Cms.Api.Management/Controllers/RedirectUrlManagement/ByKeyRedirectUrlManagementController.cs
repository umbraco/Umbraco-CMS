﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

public class ByKeyRedirectUrlManagementController : RedirectUrlManagementBaseController
{
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IRedirectUrlPresentationFactory _redirectUrlPresentationFactory;

    public ByKeyRedirectUrlManagementController(
        IRedirectUrlService redirectUrlService,
        IRedirectUrlPresentationFactory redirectUrlPresentationFactory)
    {
        _redirectUrlService = redirectUrlService;
        _redirectUrlPresentationFactory = redirectUrlPresentationFactory;
    }

    [HttpGet("{key:guid}")]
    [ProducesResponseType(typeof(PagedViewModel<RedirectUrlResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<PagedViewModel<RedirectUrlResponseModel>>> ByKey(Guid key, int skip, int take)
    {
        IRedirectUrl[] redirects = _redirectUrlService.GetContentRedirectUrls(key).ToArray();

        IEnumerable<RedirectUrlResponseModel> viewModels = _redirectUrlPresentationFactory.CreateMany(redirects);

        return Task.FromResult<ActionResult<PagedViewModel<RedirectUrlResponseModel>>>(new PagedViewModel<RedirectUrlResponseModel>
        {
            Items = viewModels.Skip(skip).Take(take),
            Total = redirects.Length,
        });
    }
}
