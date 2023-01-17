using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core.Security;
using IDictionaryService = Umbraco.Cms.Api.Management.Services.Dictionary.IDictionaryService;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class UpdateDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDictionaryFactory _dictionaryFactory;
    private readonly IDictionaryService _dictionaryService;

    public UpdateDictionaryController(
        ILocalizationService localizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDictionaryFactory dictionaryFactory,
        IDictionaryService dictionaryService)
    {
        _localizationService = localizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _dictionaryFactory = dictionaryFactory;
        _dictionaryService = dictionaryService;
    }

    [HttpPut("{key:Guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid key, DictionaryItemUpdateModel dictionaryItemUpdateModel)
    {
        IDictionaryItem? current = _localizationService.GetDictionaryItemById(key);
        if (current is null)
        {
            return NotFound();
        }

        ProblemDetails? collision = _dictionaryService.DetectNamingCollision(dictionaryItemUpdateModel.Name, current);
        if (collision != null)
        {
            return Conflict(collision);
        }

        IDictionaryItem updated = _dictionaryFactory.MapDictionaryItemUpdate(current, dictionaryItemUpdateModel);
        _localizationService.Save(updated, CurrentUserId(_backOfficeSecurityAccessor));

        return await Task.FromResult(Ok());
    }
}
