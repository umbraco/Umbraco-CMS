using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;


public class EnterPreviewController : PreviewControllerBase
{
    private readonly ICookieManager _cookieManager;

    public EnterPreviewController(ICookieManager cookieManager) => _cookieManager = cookieManager;

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Enter()
    {
        _cookieManager.SetCookieValue(Constants.Web.PreviewCookieName, "preview");

        return Ok();
    }
}
