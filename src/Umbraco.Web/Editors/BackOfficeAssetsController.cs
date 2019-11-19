using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class BackOfficeAssetsController : UmbracoAuthorizedJsonController
    {
        private readonly IFileSystem _jsLibFileSystem;

        public BackOfficeAssetsController(IIOHelper ioHelper, ILogger logger, IGlobalSettings globalSettings)
        {
            _jsLibFileSystem = new PhysicalFileSystem(ioHelper, logger, globalSettings.UmbracoPath + Current.IOHelper.DirSepChar + "lib");
        }



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
