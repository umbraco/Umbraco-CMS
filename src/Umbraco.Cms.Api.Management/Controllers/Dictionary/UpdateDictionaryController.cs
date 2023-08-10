using Asp.Versioning;
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

[ApiVersion("1.0")]
public class UpdateDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDictionaryPresentationFactory _dictionaryPresentationFactory;

    public UpdateDictionaryController(
        IDictionaryItemService dictionaryItemService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDictionaryPresentationFactory dictionaryPresentationFactory)
    {
        _dictionaryItemService = dictionaryItemService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _dictionaryPresentationFactory = dictionaryPresentationFactory;
    }

    [HttpPut($"{{{nameof(id)}:guid}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateDictionaryItemRequestModel updateDictionaryItemRequestModel)
    {
        IDictionaryItem? current = await _dictionaryItemService.GetAsync(id);
        if (current == null)
        {
            return DictionaryNotFound();
        }

        IDictionaryItem updated = await _dictionaryPresentationFactory.MapUpdateModelToDictionaryItemAsync(current, updateDictionaryItemRequestModel);

        Attempt<IDictionaryItem, DictionaryItemOperationStatus> result =
            await _dictionaryItemService.UpdateAsync(updated, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : DictionaryItemOperationStatusResult(result.Status);
    }
}
