using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Content)]   
    public class TourController : UmbracoAuthorizedJsonController
    {
        //TODO: Strongly type this for final release!
        public JArray GetTours()
        {
            //TODO: Add error checking to this for final release!

            var result = new JArray();

            if (UmbracoConfig.For.UmbracoSettings().BackOffice.Tours.EnableTours == false)
                return result;

            var tourFiles = Directory.GetFiles(
                    Path.Combine(IOHelper.MapPath(SystemDirectories.Config), "BackOfficeTours"), "*.json")
                .OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase);
            foreach (var tourFile in tourFiles)
            {
                var contents = File.ReadAllText(tourFile);
                result.Add(JArray.Parse(contents));
            }

            return result;
        }
    }
}