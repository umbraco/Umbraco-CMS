using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiVersion("1.0")]
public class ConfigurationMediaController : MediaControllerBase
{
    private readonly GlobalSettings _globalSettings;
    private readonly ContentSettings _contentSettings;

    public ConfigurationMediaController(IOptionsSnapshot<GlobalSettings> globalSettings, IOptionsSnapshot<ContentSettings> contentSettings)
    {
        _contentSettings = contentSettings.Value;
        _globalSettings = globalSettings.Value;
    }

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MediaConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration()
    {
        var responseModel = new MediaConfigurationResponseModel
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            SanitizeTinyMce = _globalSettings.SanitizeTinyMce,
        };
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
