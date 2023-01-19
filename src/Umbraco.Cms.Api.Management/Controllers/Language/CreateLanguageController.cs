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

public class CreateLanguageController : LanguageControllerBase
{
    private readonly ILanguageFactory _languageFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateLanguageController(
        ILanguageFactory languageFactory,
        ILocalizationService localizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _languageFactory = languageFactory;
        _localizationService = localizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(LanguageCreateModel languageCreateModel)
    {
        ILanguage created = _languageFactory.MapCreateModelToLanguage(languageCreateModel);

        Attempt<ILanguage, LanguageOperationStatus> result = _localizationService.Create(created, CurrentUserId(_backOfficeSecurityAccessor));

        if (result.Success)
        {
            return await Task.FromResult(
                CreatedAtAction<ByIsoCodeLanguageController>(
                    controller => nameof(controller.ByIsoCode),
                    new { isoCode = result.Result.IsoCode }));
        }

        return LanguageOperationStatusResult(result.Status);
    }
}
