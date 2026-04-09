using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

/// <summary>
/// Provides API endpoints for moving dictionary items within the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
public class MoveDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveDictionaryController"/> class, responsible for handling dictionary item move operations in the management API.
    /// </summary>
    /// <param name="dictionaryItemService">Service used to manage dictionary items.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public MoveDictionaryController(IDictionaryItemService dictionaryItemService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dictionaryItemService = dictionaryItemService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Moves a dictionary.")]
    [EndpointDescription("Moves a dictionary identified by the provided Id to a different location.")]
    public async Task<IActionResult> Move(
        CancellationToken cancellationToken,
        Guid id,
        MoveDictionaryRequestModel moveDictionaryRequestModel)
    {
        IDictionaryItem? source = await _dictionaryItemService.GetAsync(id);
        if (source == null)
        {
            return DictionaryItemNotFound();
        }

        Attempt<IDictionaryItem, DictionaryItemOperationStatus> result = await _dictionaryItemService.MoveAsync(
            source,
            moveDictionaryRequestModel.Target?.Id,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : DictionaryItemOperationStatusResult(result.Status);
    }
}
