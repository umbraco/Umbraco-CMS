using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class DeleteDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteDictionaryController(ILocalizationService localizationService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _localizationService = localizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete("{key}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid key)
    {
        IDictionaryItem? dictionaryItem = _localizationService.GetDictionaryItemById(key);
        if (dictionaryItem == null)
        {
            return NotFound();
        }

        _localizationService.Delete(dictionaryItem, CurrentUserId(_backOfficeSecurityAccessor));
        return await Task.FromResult(Ok());
    }
}
