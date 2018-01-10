using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Content)]
    public class TourController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<BackOfficeTourFile> GetTours()
        {
            var result = new List<BackOfficeTourFile>();

            if (UmbracoConfig.For.UmbracoSettings().BackOffice.Tours.EnableTours == false)
                return result;

            //add core tour files
            var coreToursPath = Path.Combine(IOHelper.MapPath(SystemDirectories.Config), "BackOfficeTours");
            if (Directory.Exists(coreToursPath))
            {
                var coreTourFiles = Directory.GetFiles(coreToursPath, "*.json");

                foreach (var tourFile in coreTourFiles)
                {
                    TryParseTourFile(tourFile, result);
                }
            }

            //collect all tour files in packages
            foreach (var plugin in Directory.EnumerateDirectories(IOHelper.MapPath(SystemDirectories.AppPlugins)))
            {
                var pluginName = Path.GetFileName(plugin.TrimEnd('\\'));

                foreach (var backofficeDir in Directory.EnumerateDirectories(plugin, "backoffice"))
                {
                    foreach (var tourDir in Directory.EnumerateDirectories(backofficeDir, "tours"))
                    {
                        foreach (var tourFile in Directory.EnumerateFiles(tourDir, "*.json"))
                        {
                            TryParseTourFile(tourFile, result, pluginName);
                        }
                    }
                }
            }

            return result.OrderBy(x => x.FileName, StringComparer.InvariantCultureIgnoreCase);
        }

        private void TryParseTourFile(string tourFile, List<BackOfficeTourFile> result, string pluginName = null)
        {
            try
            {
                var contents = File.ReadAllText(tourFile);
                var tours = JsonConvert.DeserializeObject<BackOfficeTour[]>(contents);
                var disabledTours = TourFilterResolver.Current.DisabledTours;

                result.Add(new BackOfficeTourFile
                {
                    FileName = Path.GetFileNameWithoutExtension(tourFile),
                    PluginName = pluginName,
                    Tours = tours
                        .Where(x => disabledTours.Contains(x.Alias, StringComparer.InvariantCultureIgnoreCase) == false)
                        .ToArray()
                });
            }
            catch (IOException e)
            {
                throw new IOException("Error while trying to read file: " + tourFile, e);
            }
            catch (JsonReaderException e)
            {
                throw new JsonReaderException("Error while trying to parse content as tour data: " + tourFile, e);
            }
        }
    }
}