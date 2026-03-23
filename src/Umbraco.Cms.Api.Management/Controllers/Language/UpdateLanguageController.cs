using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

/// <summary>
/// Controller responsible for handling requests to update language resources in the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
public class UpdateLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLanguageController"/> class, responsible for handling language update operations in the Umbraco management API.
    /// </summary>
    /// <param name="languageService">Service used to manage and update language entities.</param>
    /// <param name="umbracoMapper">Mapper used to convert between domain models and API models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context, used for authorization and user information.</param>
    public UpdateLanguageController(
        ILanguageService languageService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _languageService = languageService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut($"{{{nameof(isoCode)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Updates a language.")]
    [EndpointDescription("Updates a language identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        string isoCode,
        UpdateLanguageRequestModel updateLanguageRequestModel)
    {
        ILanguage? current = await _languageService.GetAsync(isoCode);
        if (current is null)
        {
            return LanguageNotFound();
        }

        ILanguage updated = _umbracoMapper.Map(updateLanguageRequestModel, current);

        Attempt<ILanguage, LanguageOperationStatus> result = await _languageService.UpdateAsync(updated, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : LanguageOperationStatusResult(result.Status);
    }
}
