using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
public class DeleteLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteLanguageController(ILanguageService languageService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _languageService = languageService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete($"{{{nameof(isoCode)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string isoCode)
    {
        Attempt<ILanguage?, LanguageOperationStatus> result = await _languageService.DeleteAsync(isoCode, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : LanguageOperationStatusResult(result.Status);
    }
}
