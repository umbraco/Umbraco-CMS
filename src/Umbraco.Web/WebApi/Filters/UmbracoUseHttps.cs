using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;
using System.Web.Mvc;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Web.WebApi.Filters
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use the filter Umbraco.Web.Mvc.UmbracoRequireHttpsAttribute instead, this one is in the wrong namespace")]
    public class UmbracoUseHttps : Umbraco.Web.Mvc.UmbracoRequireHttpsAttribute
    {
    }
}
