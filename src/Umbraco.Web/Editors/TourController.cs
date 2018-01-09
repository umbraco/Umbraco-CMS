using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
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
        public IEnumerable<Tour[]> GetTours()
        {
            var tours = new List<Tour[]>();

            if (UmbracoConfig.For.UmbracoSettings().BackOffice.Tours.EnableTours == false)
                return tours;

            var toursPath = Path.Combine(IOHelper.MapPath(SystemDirectories.Config), "BackOfficeTours");
            if (Directory.Exists(toursPath) == false)
                return tours;

            var tourFiles = Directory.GetFiles(toursPath, "*.json")
                .OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase);
            var disabledTours = TourFilterResolver.Current.DisabledTours;

            foreach (var tourFile in tourFiles)
            {
                try
                {
                    var contents = File.ReadAllText(tourFile);
                    var tourArray = JsonConvert.DeserializeObject<Tour[]>(contents);
                    tours.Add(tourArray.Where(x =>
                        disabledTours.Contains(x.Alias, StringComparer.InvariantCultureIgnoreCase) == false).ToArray());
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

            return tours;
        }
    }
}