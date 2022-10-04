using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

public class CreateDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly GlobalSettings _globalSettings;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly ILogger<CreateDictionaryController> _logger;

    public CreateDictionaryController(
        ILocalizationService localizationService,
        ILocalizedTextService localizedTextService,
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        ILogger<CreateDictionaryController> logger)
    {
        _localizationService = localizationService;
        _localizedTextService = localizedTextService;
        _globalSettings = globalSettings.Value;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _logger = logger;
    }

    /// <summary>
    ///     Creates a new dictionary item
    /// </summary>
    /// <param name="dictionaryViewModel">The viewmodel to pass to the action</param>
    /// <returns>
    ///     The <see cref="HttpResponseMessage" />.
    /// </returns>
    [HttpPost("create")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create(DictionaryItemViewModel dictionaryViewModel)
    {
        if (string.IsNullOrEmpty(dictionaryViewModel.Key.ToString()))
        {
            return ValidationProblem("Key can not be empty."); // TODO: translate
        }

        if (_localizationService.DictionaryItemExists(dictionaryViewModel.Key.ToString()))
        {
            var message = _localizedTextService.Localize(
                "dictionaryItem",
                "changeKeyError",
                _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.GetUserCulture(_localizedTextService, _globalSettings),
                new Dictionary<string, string?>
                {
                    { "0", dictionaryViewModel.Key.ToString() },
                });
            return await Task.FromResult(ValidationProblem(message));
        }

        try
        {
            Guid? parentGuid = null;

            if (dictionaryViewModel.ParentId.HasValue)
            {
                parentGuid = dictionaryViewModel.ParentId;
            }

            IDictionaryItem item = _localizationService.CreateDictionaryItemWithIdentity(
                dictionaryViewModel.Key.ToString(),
                parentGuid,
                string.Empty);


            return await Task.FromResult(Created($"api/v1.0/dictionary/{item.Key}", item.Key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dictionary with {Name} under {ParentId}", dictionaryViewModel.Key, dictionaryViewModel.ParentId);
            return await Task.FromResult(ValidationProblem("Error creating dictionary item"));
        }
    }
}
