using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class UpdateDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDictionaryFactory _dictionaryFactory;

    public UpdateDictionaryController(
        ILocalizationService localizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDictionaryFactory dictionaryFactory)
    {
        _localizationService = localizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _dictionaryFactory = dictionaryFactory;
    }

    [HttpPut("{key:Guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid key, DictionaryItemUpdateModel dictionaryItemUpdateModel)
    {
        IDictionaryItem updated = _dictionaryFactory.MapUpdateModelToDictionaryItem(key, dictionaryItemUpdateModel);

        Attempt<IDictionaryItem, DictionaryItemOperationStatus> result = _localizationService.Update(updated, CurrentUserId(_backOfficeSecurityAccessor));

        if (result.Success)
        {
            return await Task.FromResult(Ok());
        }

        return DictionaryItemOperationStatusResult(result.Status);
    }
}
