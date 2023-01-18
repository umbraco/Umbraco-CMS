using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class CreateDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDictionaryFactory _dictionaryFactory;

    public CreateDictionaryController(
        ILocalizationService localizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDictionaryFactory dictionaryFactory)
    {
        _localizationService = localizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _dictionaryFactory = dictionaryFactory;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(DictionaryItemCreateModel dictionaryItemCreateModel)
    {
        IEnumerable<IDictionaryTranslation> translations = _dictionaryFactory.MapTranslations(dictionaryItemCreateModel.Translations);

        Attempt<IDictionaryItem?, DictionaryItemOperationStatus> result = _localizationService.Create(
            dictionaryItemCreateModel.Name,
            dictionaryItemCreateModel.ParentKey,
            translations,
            CurrentUserId(_backOfficeSecurityAccessor));

        if (result.Success)
        {
            return await Task.FromResult(CreatedAtAction<ByKeyDictionaryController>(controller => nameof(controller.ByKey), result.Result!.Key));
        }

        return DictionaryItemOperationStatusResult(result.Status);
    }
}
