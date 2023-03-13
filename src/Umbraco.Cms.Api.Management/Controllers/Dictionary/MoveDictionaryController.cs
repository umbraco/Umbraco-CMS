using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class MoveDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public MoveDictionaryController(IDictionaryItemService dictionaryItemService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dictionaryItemService = dictionaryItemService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost("{key:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Move(Guid key, MoveDictionaryRequestModel moveDictionaryRequestModel)
    {
        IDictionaryItem? source = await _dictionaryItemService.GetAsync(key);
        if (source == null)
        {
            return NotFound();
        }

        Attempt<IDictionaryItem, DictionaryItemOperationStatus> result = await _dictionaryItemService.MoveAsync(
            source,
            moveDictionaryRequestModel.TargetKey,
            CurrentUserId(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : DictionaryItemOperationStatusResult(result.Status);
    }
}
