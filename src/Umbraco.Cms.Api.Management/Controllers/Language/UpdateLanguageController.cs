using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

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
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(string isoCode, LanguageUpdateModel languageUpdateModel)
    {
        ILanguage? current = await _languageService.GetAsync(isoCode);
        if (current is null)
        {
            return NotFound();
        }

        ILanguage updated = _umbracoMapper.Map(languageUpdateModel, current);

        Attempt<ILanguage, LanguageOperationStatus> result = await _languageService.UpdateAsync(updated, CurrentUserId(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : LanguageOperationStatusResult(result.Status);
    }
}
