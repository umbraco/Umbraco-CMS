using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

    /// <summary>
    /// API controller responsible for handling requests to create new dictionary items in Umbraco CMS.
    /// </summary>
[ApiVersion("1.0")]
public class CreateDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDictionaryPresentationFactory _dictionaryPresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Dictionary.CreateDictionaryController"/> class,
    /// providing dependencies required for managing dictionary items in the Umbraco back office API.
    /// </summary>
    /// <param name="dictionaryItemService">Service for managing dictionary items.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="dictionaryPresentationFactory">Factory for creating dictionary presentation models.</param>
    /// <param name="authorizationService">Service for handling authorization checks.</param>
    public CreateDictionaryController(
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
    /// Creates a new dictionary item using the details provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="createDictionaryItemRequestModel">The model containing the details of the dictionary item to create, including translations.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the create operation, including possible status codes for success or failure.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Creates a new dictionary.")]
    [EndpointDescription("Creates a new dictionary with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateDictionaryItemRequestModel createDictionaryItemRequestModel)
    {
        IDictionaryItem created = await _dictionaryPresentationFactory.MapCreateModelToDictionaryItemAsync(createDictionaryItemRequestModel);

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            new DictionaryPermissionResource(createDictionaryItemRequestModel.Translations.Select(t => t.IsoCode)),
            AuthorizationPolicies.DictionaryPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<IDictionaryItem, DictionaryItemOperationStatus> result =
            await _dictionaryItemService.CreateAsync(created, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyDictionaryController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : DictionaryItemOperationStatusResult(result.Status);
    }
}
