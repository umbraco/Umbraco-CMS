using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

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
    /// <param name="key">The key of the dictionary item to delete</param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    [HttpDelete("{key}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid key)
    {
        IDictionaryItem? foundDictionary = _localizationService.GetDictionaryItemByKey(key.ToString());

        if (foundDictionary == null)
        {
            return await Task.FromResult(NotFound());
        }

        IEnumerable<IDictionaryItem> foundDictionaryDescendants =
            _localizationService.GetDictionaryItemDescendants(foundDictionary.Key);

        foreach (IDictionaryItem dictionaryItem in foundDictionaryDescendants)
        {
            _localizationService.Delete(dictionaryItem, _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);
        }

        _localizationService.Delete(foundDictionary, _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);

        return await Task.FromResult(Ok());
    }
}
