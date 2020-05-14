using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Controllers;

namespace Umbraco.Web.UI.NetCore.Controllers
{
    [PluginController("Test")]
    [IsBackOffice]
    public class TestBackOfficeUmbracoApiController : UmbracoApiController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Content("hello world");
        }
    }
}
