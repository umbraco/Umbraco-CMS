using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

[ApiVersion("1.0")]
public class ConfigurationMemberController : MemberControllerBase
{
    private readonly GlobalSettings _globalSettings;
    private readonly ContentSettings _contentSettings;
    private readonly PropertySettings _propertySettings;

    public ConfigurationMemberController(IOptionsSnapshot<GlobalSettings> globalSettings, IOptionsSnapshot<ContentSettings> contentSettings, IOptionsSnapshot<PropertySettings> propertySettings)
    {
        _contentSettings = contentSettings.Value;
        _globalSettings = globalSettings.Value;
        _propertySettings = propertySettings.Value;
    }

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration()
    {
        var responseModel = new MemberConfigurationResponseModel
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            SanitizeTinyMce = _globalSettings.SanitizeTinyMce,
            StandardFieldNames = _propertySettings.StandardFieldNames,
        };
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
