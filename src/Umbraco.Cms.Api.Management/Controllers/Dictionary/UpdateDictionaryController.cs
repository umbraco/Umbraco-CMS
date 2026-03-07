using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Dictionary;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

/// <summary>
/// API controller responsible for handling requests to update dictionary items in the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
public class UpdateDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDictionaryPresentationFactory _dictionaryPresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDictionaryController"/> class, which handles API requests for updating dictionary items in Umbraco.
    /// </summary>
    /// <param name="dictionaryItemService">Service for managing dictionary items.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="dictionaryPresentationFactory">Factory for creating dictionary item presentation models.</param>
    /// <param name="authorizationService">Service for handling authorization checks.</param>
    public UpdateDictionaryController(
        IDictionaryItemService dictionaryItemService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDictionaryPresentationFactory dictionaryPresentationFactory,
        IAuthorizationService authorizationService)
    {
        _dictionaryItemService = dictionaryItemService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _dictionaryPresentationFactory = dictionaryPresentationFactory;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Updates an existing dictionary item with the specified identifier using the provided details.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the dictionary item to update.</param>
    /// <param name="updateDictionaryItemRequestModel">The model containing the updated dictionary item details.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.</returns
    [HttpPut($"{{{nameof(id)}:guid}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a dictionary.")]
    [EndpointDescription("Updates a dictionary identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateDictionaryItemRequestModel updateDictionaryItemRequestModel)
    {
        IDictionaryItem? current = await _dictionaryItemService.GetAsync(id);
        if (current == null)
        {
            return DictionaryItemNotFound();
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            new DictionaryPermissionResource(updateDictionaryItemRequestModel.Translations.Select(t => t.IsoCode)),
            AuthorizationPolicies.DictionaryPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        IDictionaryItem updated = await _dictionaryPresentationFactory.MapUpdateModelToDictionaryItemAsync(current, updateDictionaryItemRequestModel);

        Attempt<IDictionaryItem, DictionaryItemOperationStatus> result =
            await _dictionaryItemService.UpdateAsync(updated, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : DictionaryItemOperationStatusResult(result.Status);
    }
}
