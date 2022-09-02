using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

[ApiVersion("1.0")]
public class DeleteDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteDictionaryController(ILocalizationService localizationService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _localizationService = localizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }
    /// <summary>
    ///     Deletes a data type with a given ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        IDictionaryItem? foundDictionary = _localizationService.GetDictionaryItemById(id);

        if (foundDictionary == null)
        {
            return NotFound();
        }

        IEnumerable<IDictionaryItem> foundDictionaryDescendants =
            _localizationService.GetDictionaryItemDescendants(foundDictionary.Key);

        foreach (IDictionaryItem dictionaryItem in foundDictionaryDescendants)
        {
            _localizationService.Delete(dictionaryItem, _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);
        }

        _localizationService.Delete(foundDictionary, _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);

        return Ok();
    }
}
