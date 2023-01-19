using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

public class UpdateLanguageController : LanguageControllerBase
{
    private readonly ILanguageFactory _languageFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateLanguageController(
        ILanguageFactory languageFactory,
        ILocalizationService localizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _languageFactory = languageFactory;
        _localizationService = localizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{isoCode}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(string isoCode, LanguageUpdateModel languageUpdateModel)
    {
        ILanguage? current = _localizationService.GetLanguageByIsoCode(isoCode);
        if (current is null)
        {
            return NotFound();
        }

        ILanguage updated = _languageFactory.MapUpdateModelToLanguage(current, languageUpdateModel);

        Attempt<ILanguage, LanguageOperationStatus> result = _localizationService.Update(updated, CurrentUserId(_backOfficeSecurityAccessor));

        if (result.Success)
        {
            return await Task.FromResult(Ok());
        }

        return LanguageOperationStatusResult(result.Status);
    }
}
