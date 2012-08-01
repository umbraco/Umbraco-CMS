using System;
using System.IO;
using System.Web.Razor;
using System.Web.WebPages.Razor;

namespace umbraco.MacroEngines
{
    public class RazorUmbracoHost : WebPageRazorHost {
        public RazorUmbracoHost(string virtualPath) : base(virtualPath) {}
        public RazorUmbracoHost(string virtualPath, string physicalPath) : base(virtualPath, physicalPath) {}

        protected override RazorCodeLanguage GetCodeLanguage() {
            var ext = Path.GetExtension(VirtualPath);
            if (string.Equals(ext, ".razor", StringComparison.OrdinalIgnoreCase))
                return new CSharpRazorCodeLanguage();
            return base.GetCodeLanguage();
        }


    }
}
