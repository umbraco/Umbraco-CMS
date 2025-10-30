﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Language.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Item;

[ApiVersion("1.0")]
public class DefaultLanguageEntityController : LanguageItemControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _mapper;

    public DefaultLanguageEntityController(ILanguageService languageService, IUmbracoMapper mapper)
    {
        _languageService = languageService;
        _mapper = mapper;
    }

    [HttpGet("default")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(LanguageItemResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the default language.")]
    [EndpointDescription("Gets the default language configured for the Umbraco installation.")]
    public async Task<IActionResult> Default(CancellationToken cancellationToken)
    {
        ILanguage? language = await _languageService.GetDefaultLanguageAsync();
        return language is not null
            ? Ok(_mapper.Map<ILanguage, LanguageItemResponseModel>(language))
            : OperationStatusResult(
                LanguageOperationStatus.NotFound,
                problemDetailsBuilder => NotFound(problemDetailsBuilder.WithTitle("The default language could not be found.")));
    }
}
