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
public class UpdateLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateLanguageController(
        ILanguageService languageService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _languageService = languageService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut($"{{{nameof(isoCode)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        string isoCode,
        UpdateLanguageRequestModel updateLanguageRequestModel)
    {
        ILanguage? current = await _languageService.GetAsync(isoCode);
        if (current is null)
        {
            return LanguageNotFound();
        }

        ILanguage updated = _umbracoMapper.Map(updateLanguageRequestModel, current);

        Attempt<ILanguage, LanguageOperationStatus> result = await _languageService.UpdateAsync(updated, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : LanguageOperationStatusResult(result.Status);
    }
}
