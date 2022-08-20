using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace UmbracoPackage._1.Controllers;

public class DashboardApiController : UmbracoAuthorizedApiController
{
    /// <summary>
    ///  GetApi - Called in our ServerVariablesParser.Parsing event handler
    ///  this gets the URL of this API, so we don't have to hardwire it anywhere
    /// </summary>
    [HttpGet]
    public bool GetApi() => true; 

    /// <summary>
    ///  Simple call return the time,
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public DateTime GetServerInfo() => DateTime.UtcNow;
}