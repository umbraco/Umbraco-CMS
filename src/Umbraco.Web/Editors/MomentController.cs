using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Umbraco.Core.IO;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class MomentController : UmbracoAuthorizedJsonController
    {
        
        [HttpGet]
        public IEnumerable<string> GetSupportedLocales()
        {
            var momentLocaleFolder = "moment";
            var fileSystem = FileSystemProviderManager.Current.LibFileSystem;
            var cultures = fileSystem.GetFiles(momentLocaleFolder, "*.js").ToList();
            for (var i = 0; i < cultures.Count(); i++)
            {
                cultures[i] = cultures[i]
                    .Substring(cultures[i].IndexOf(momentLocaleFolder) + momentLocaleFolder.Length + 1);
            }
            return cultures;
        }

    }
}
