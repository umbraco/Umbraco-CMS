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
        public IEnumerable<string> GetSupportedMomentLocales()
        {
            const string momentLocaleFolder = "moment";
            var cultures = _jsLibFileSystem.GetFiles(momentLocaleFolder, "*.js").ToList();
            for (var i = 0; i < cultures.Count; i++)
            {
                cultures[i] = cultures[i]
                    .Substring(cultures[i].IndexOf(momentLocaleFolder, StringComparison.Ordinal) + momentLocaleFolder.Length + 1);
            }
            return cultures;
        }
    }
}
