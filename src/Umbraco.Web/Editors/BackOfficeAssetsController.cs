using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Umbraco.Core.IO;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class BackOfficeAssetsController : UmbracoAuthorizedJsonController
    {
        
        [HttpGet]
        public IEnumerable<string> GetSupportedMomentLocales()
        {
            const string momentLocaleFolder = "moment";
            var fileSystem = FileSystemProviderManager.Current.JavaScriptLibraryFileSystem;
            var cultures = fileSystem.GetFiles(momentLocaleFolder, "*.js").ToList();
            for (var i = 0; i < cultures.Count; i++)
            {
                cultures[i] = cultures[i]
                    .Substring(cultures[i].IndexOf(momentLocaleFolder, StringComparison.Ordinal) + momentLocaleFolder.Length + 1);
            }
            return cultures;
        }
    }
}
