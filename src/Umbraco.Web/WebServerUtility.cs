using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using Umbraco.Core;
using Microsoft.Web.Administration;

namespace Umbraco.Web
{
    internal class WebServerUtility
    {
        // NOTE
        //
        // there's some confusion with Microsoft.Web.Administration versions
        // 7.0.0.0 is installed by NuGet and will read IIS settings
        // 7.9.0.0 comes with IIS Express and will read IIS Express
        // we want to use 7.0.0.0 when building
        // and then... there are further versions that are N/A on NuGet
        //
        // Umbraco uses 7.0.0.0 from NuGet
        // IMPORTANT: and then, the reference's SpecificVersion property MUST be set to true
        // otherwise we might build with 7.9.0.0 and end up in troubles (reading IIS Express
        // instead of IIS even when running IIS) - IIS Express has a binding redirect from
        // 7.0.0.0 to 7.9.0.0 so it's fine.
        //
        // read:
        // http://stackoverflow.com/questions/11208270/microsoft-web-administration-servermanager-looking-in-wrong-directory-for-iisexp
        // http://stackoverflow.com/questions/8467908/how-to-use-servermanager-to-read-iis-sites-not-iis-express-from-class-library
        // http://stackoverflow.com/questions/25812169/microsoft-web-administration-servermanager-is-connecting-to-the-iis-express-inst

        public static IEnumerable<Uri> GetBindings()
        {
            // FIXME
            // which of these methods shall we use?
            // what about permissions, trust, etc?

            //return GetBindings2();
            throw new NotImplementedException();
        }

        private static IEnumerable<Uri> GetBindings1()
        {
            // get the site name  
            var siteName = HostingEnvironment.SiteName;

            // get the site from the sites section from the AppPool.config 
            var sitesSection = WebConfigurationManager.GetSection(null, null, "system.applicationHost/sites");
            var site = sitesSection.GetCollection().FirstOrDefault(x => ((string) x["name"]).InvariantEquals(siteName));
            if (site == null)
                return Enumerable.Empty<Uri>();

            return site.GetCollection("bindings")
                .Where(x => ((string) x["protocol"]).StartsWith("http", StringComparison.OrdinalIgnoreCase))
                .Select(x =>
                {
                    var bindingInfo = (string) x["bindingInformation"];
                    var parts = bindingInfo.Split(':'); // can count be != 3 ??
                    return new Uri(x["protocol"] + "://" + parts[2] + ":" + parts[1] + "/");
                });
        }

        private static IEnumerable<Uri> GetBindings2()
        {
            // get the site name  
            var siteName = HostingEnvironment.SiteName;

            // get the site from the server manager
            var mgr = new ServerManager();
            var site = mgr.Sites.FirstOrDefault(x => x.Name.InvariantEquals(siteName));
            if (site == null)
                return Enumerable.Empty<Uri>();

            // get the bindings
            return site.Bindings
                .Where(x => x.Protocol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                .Select(x => new Uri(x.Protocol + "://" + x.Host + ":" + x.EndPoint.Port + "/"));
        }
    }
}
