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
        private readonly IFileSystem _jsLibFileSystem = new PhysicalFileSystem(SystemDirectories.Umbraco + IOHelper.DirSepChar + "lib");
        
        [HttpGet]
        public object GetSupportedLocales()
        {
            const string momentLocaleFolder = "moment";
            const string flatpickrLocaleFolder = "flatpickr/l10n";

            return new
            {
                moment = GetLocales(momentLocaleFolder),
                flatpickr = GetLocales(flatpickrLocaleFolder)
            };
        }

        private IEnumerable<string> GetLocales(string path)
        {
            var cultures = _jsLibFileSystem.GetFiles(path, "*.js").ToList();
            for (var i = 0; i < cultures.Count; i++)
            {
                cultures[i] = cultures[i]
                    .Substring(cultures[i].IndexOf(path, StringComparison.Ordinal) + path.Length + 1);
            }
            return cultures;
        }
    }
}
