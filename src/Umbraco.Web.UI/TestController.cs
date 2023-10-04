using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.UI;

public class TestController : UmbracoApiController
{
    public IActionResult GetAllProducts() => Ok();
}
