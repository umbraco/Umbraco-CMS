using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

/// <summary>
/// Controller responsible for handling requests to delete languages in the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
public class DeleteLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLanguageController"/> class.
    /// </summary>
    /// <param name="languageService">
    /// The service used to manage language-related operations.
    /// </param>
    /// <param name="backOfficeSecurityAccessor">
    /// Provides access to back office security features for authorization and authentication.
    /// </param>
    public DeleteLanguageController(ILanguageService languageService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _languageService = languageService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Deletes a language identified by the provided ISO code.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="isoCode">The ISO code of the language to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns <c>200 OK</c> if the language was deleted, <c>400 Bad Request</c> if the request is invalid, or <c>404 Not Found</c> if the language does not exist.</returns>
    [HttpDelete($"{{{nameof(isoCode)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Deletes a language.")]
    [EndpointDescription("Deletes a language identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string isoCode)
    {
        Attempt<ILanguage?, LanguageOperationStatus> result = await _languageService.DeleteAsync(isoCode, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : LanguageOperationStatusResult(result.Status);
    }
}
