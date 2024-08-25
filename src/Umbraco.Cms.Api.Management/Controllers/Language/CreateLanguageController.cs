using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
public class CreateLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateLanguageController(
        ILanguageService languageService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _languageService = languageService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateLanguageRequestModel createLanguageRequestModel)
    {
        ILanguage created = _umbracoMapper.Map<ILanguage>(createLanguageRequestModel)!;

        Attempt<ILanguage, LanguageOperationStatus> result = await _languageService.CreateAsync(created, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByIsoCodeLanguageController>(controller => nameof(controller.ByIsoCode), new { isoCode = result.Result.IsoCode }, result.Result.IsoCode)
            : LanguageOperationStatusResult(result.Status);
    }
}
