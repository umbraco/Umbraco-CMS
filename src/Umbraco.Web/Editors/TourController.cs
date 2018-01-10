using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

            var toursPath = Path.Combine(IOHelper.MapPath(SystemDirectories.Config), "BackOfficeTours");
            if (Directory.Exists(toursPath) == false)
                return result;

            var tourFiles = Directory.GetFiles(toursPath, "*.json")
                .OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase);
            var disabledTours = TourFilterResolver.Current.DisabledTours;
                
            var coreTourFiles = Directory.GetFiles(
                Path.Combine(IOHelper.MapPath(SystemDirectories.Config), "BackOfficeTours"), "*.json");

            foreach (var tourFile in coreTourFiles)
            {
            	try
            	{
                var contents = File.ReadAllText(tourFile);

                result.Add(new BackOfficeTourFile
                {
                    FileName = Path.GetFileNameWithoutExtension(tourFile),
                    Tours = JsonConvert.DeserializeObject<BackOfficeTour[]>(contents)
                });
                }
                catch (IOException e)
                {
                    Logger.Error<TourController>("Error while trying to read file: " + tourFile, e);
                    throw new IOException("Error while trying to read file: " + tourFile, e);
                }
                catch (JsonReaderException e)
                {
                    Logger.Error<TourController>("Error while trying to parse content as tour data: " + tourFile, e);
                    throw new JsonReaderException("Error while trying to parse content as tour data: " + tourFile, e);
                }

            }

            //collect all tour files in packges
            foreach (var plugin in Directory.EnumerateDirectories(IOHelper.MapPath(SystemDirectories.AppPlugins)))
            {
                var pluginName = Path.GetFileName(plugin.TrimEnd('\\'));

                foreach (var backofficeDir in Directory.EnumerateDirectories(plugin, "backoffice"))
                {
                    foreach (var tourDir in Directory.EnumerateDirectories(backofficeDir, "tours"))
                    {
                        foreach (var tourFile in Directory.EnumerateFiles(tourDir, "*.json"))
                        {
                            var contents = File.ReadAllText(tourFile);
                            result.Add(new BackOfficeTourFile
                            {
                                FileName = Path.GetFileNameWithoutExtension(tourFile),
                                PluginName = pluginName,
                                Tours = JsonConvert.DeserializeObject<BackOfficeTour[]>(contents)
                            });
                        }
                    }
                }
            }

            return result.OrderBy(x => x.FileName, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}