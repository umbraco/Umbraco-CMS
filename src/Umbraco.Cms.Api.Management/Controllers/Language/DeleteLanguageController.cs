using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

public class DeleteLanguageController : LanguageControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteLanguageController(ILocalizationService localizationService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _localizationService = localizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete($"{{{nameof(isoCode)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string isoCode)
    {
        Attempt<ILanguage?, LanguageOperationStatus> result = _localizationService.Delete(isoCode, CurrentUserId(_backOfficeSecurityAccessor));

        if (result.Success)
        {
            return await Task.FromResult(Ok());
        }

        return LanguageOperationStatusResult(result.Status);
    }
}
