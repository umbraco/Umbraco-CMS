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

[ApiVersion("1.0")]
public class UpdateDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDictionaryPresentationFactory _dictionaryPresentationFactory;
    private readonly IAuthorizationService _authorizationService;

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

    [HttpPut($"{{{nameof(id)}:guid}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
