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
/// API controller responsible for handling operations related to the creation of languages (locales) in the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
public class CreateLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Language.CreateLanguageController"/> class.
    /// </summary>
    /// <param name="languageService">The service used to manage languages.</param>
    /// <param name="umbracoMapper">The mapper used for Umbraco object mapping.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features.</param>
    public CreateLanguageController(
        ILanguageService languageService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _languageService = languageService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Handles HTTP POST requests to create a new language in the system using the specified configuration.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="createLanguageRequestModel">The request model containing the configuration for the new language.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the result of the create operation:
    /// <list type="bullet">
    /// <item><description><c>201 Created</c> if the language was successfully created.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request model is invalid.</description></item>
    /// <item><description><c>404 Not Found</c> if a related resource could not be found.</description></item>
    /// </list>
    /// </returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [EndpointSummary("Creates a new language.")]
    [EndpointDescription("Creates a new language with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateLanguageRequestModel createLanguageRequestModel)
    {
        ILanguage created = _umbracoMapper.Map<ILanguage>(createLanguageRequestModel)!;

        Attempt<ILanguage, LanguageOperationStatus> result = await _languageService.CreateAsync(created, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByIsoCodeLanguageController>(controller => nameof(controller.ByIsoCode), new { isoCode = result.Result.IsoCode }, result.Result.IsoCode)
            : LanguageOperationStatusResult(result.Status);
    }
}
