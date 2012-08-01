using System.Web.WebPages.Razor;

namespace umbraco.MacroEngines {
    public class RazorUmbracoFactory : WebRazorHostFactory {
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath) {
            return new RazorUmbracoHost(virtualPath, physicalPath);
        }
    }
}
