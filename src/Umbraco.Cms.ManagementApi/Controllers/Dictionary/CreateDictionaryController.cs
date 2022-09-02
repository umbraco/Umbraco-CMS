using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

[ApiVersion("1.0")]
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
    /// <param name="parentId">
    ///     The parent id.
    /// </param>
    /// <param name="key">
    ///     The key.
    /// </param>
    /// <returns>
    ///     The <see cref="HttpResponseMessage" />.
    /// </returns>
    [HttpPost("create")]
    public async Task<ActionResult<int>> Create(int parentId, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return ValidationProblem("Key can not be empty."); // TODO: translate
        }

        if (_localizationService.DictionaryItemExists(key))
        {
            var message = _localizedTextService.Localize(
                "dictionaryItem",
                "changeKeyError",
                _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.GetUserCulture(_localizedTextService, _globalSettings),
                new Dictionary<string, string?>
                {
                    {"0", key}
                });
            return ValidationProblem(message);
        }

        try
        {
            Guid? parentGuid = null;

            if (parentId > 0)
            {
                parentGuid = _localizationService.GetDictionaryItemById(parentId)?.Key;
            }

            IDictionaryItem item = _localizationService.CreateDictionaryItemWithIdentity(
                key,
                parentGuid,
                string.Empty);


            return item.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dictionary with {Name} under {ParentId}", key, parentId);
            return ValidationProblem("Error creating dictionary item");
        }
    }
}
