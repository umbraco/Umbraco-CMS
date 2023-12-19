using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

public class EndPreviewController : PreviewControllerBase
{
    private readonly ICookieManager _cookieManager;

    public EndPreviewController(ICookieManager cookieManager) => _cookieManager = cookieManager;

    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Enter()
    {
        _cookieManager.ExpireCookie(Constants.Web.PreviewCookieName);

        // Expire Client-side cookie that determines whether the user has accepted to be in Preview Mode when visiting the website.
        _cookieManager.ExpireCookie(Constants.Web.AcceptPreviewCookieName);

        return Ok();
    }
}
