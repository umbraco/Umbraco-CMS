using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

/// <summary>
/// Controller responsible for handling requests to delete dictionary items in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class DeleteDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDictionaryController"/> class, used for handling requests to delete dictionary items in the Umbraco CMS.
    /// </summary>
    /// <param name="dictionaryItemService">Service for managing dictionary items.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public DeleteDictionaryController(IDictionaryItemService dictionaryItemService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dictionaryItemService = dictionaryItemService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete($"{{{nameof(id)}:guid}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a dictionary.")]
    [EndpointDescription("Deletes a dictionary identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IDictionaryItem?, DictionaryItemOperationStatus> result = await _dictionaryItemService.DeleteAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : DictionaryItemOperationStatusResult(result.Status);
    }
}
