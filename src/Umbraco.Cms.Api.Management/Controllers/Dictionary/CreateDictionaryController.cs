using Asp.Versioning;
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

[ApiVersion("1.0")]
public class CreateDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDictionaryPresentationFactory _dictionaryPresentationFactory;

    public CreateDictionaryController(
        IDictionaryItemService dictionaryItemService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDictionaryPresentationFactory dictionaryPresentationFactory)
    {
        _dictionaryItemService = dictionaryItemService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _dictionaryPresentationFactory = dictionaryPresentationFactory;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateDictionaryItemRequestModel createDictionaryItemRequestModel)
    {
        IDictionaryItem created = await _dictionaryPresentationFactory.MapCreateModelToDictionaryItemAsync(createDictionaryItemRequestModel);

        Attempt<IDictionaryItem, DictionaryItemOperationStatus> result =
            await _dictionaryItemService.CreateAsync(created, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByKeyDictionaryController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : DictionaryItemOperationStatusResult(result.Status);
    }
}
