using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Composing;
using System.Configuration;

// see https://github.com/Shazwazza/UmbracoScripts/tree/master/src/LoadTesting

namespace Umbraco.TestData
{
    public class LoadTestComponent : IComponent
    {
        public void Initialize()
        {
            if (ConfigurationManager.AppSettings["Umbraco.TestData.Enabled"] != "true")
                return;



            RouteTable.Routes.MapRoute(
               name: "LoadTest",
               url: "LoadTest/{action}",
               defaults: new
               {
                   controller = "LoadTest",
                   action = "Index"
               },
               namespaces: new[] { "Umbraco.TestData" }
           );
        }

        public void Terminate()
        {
        }
    }
}
