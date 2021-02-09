using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Mvc
{
    // NOTE: already migrated to netcore, just here since the below is referenced still
    public class RenderRouteHandler
    {
        // Define reserved dictionary keys for controller, action and area specified in route additional values data
        internal static class ReservedAdditionalKeys
        {
            internal const string Controller = "c";
            internal const string Action = "a";
            internal const string Area = "ar";
        }
    }
}
