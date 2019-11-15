using System;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{

    public static class GlobalSettingsExtensions
    {
        private static string _localTempPath;

        /// <summary>
        /// Gets the location of temporary files.
        /// </summary>
        public static string LocalTempPath(this IGlobalSettings globalSettings, IIOHelper ioHelper)
        {

            if (_localTempPath != null)
                return _localTempPath;

            switch (globalSettings.LocalTempStorageLocation)
            {
                case LocalTempStorage.AspNetTemp:
                    return _localTempPath = System.IO.Path.Combine(HttpRuntime.CodegenDir, "UmbracoData");

                case LocalTempStorage.EnvironmentTemp:

                    // environment temp is unique, we need a folder per site

                    // use a hash
                    // combine site name and application id
                    //  site name is a Guid on Cloud
                    //  application id is eg /LM/W3SVC/123456/ROOT
                    // the combination is unique on one server
                    // and, if a site moves from worker A to B and then back to A...
                    //  hopefully it gets a new Guid or new application id?

                    var siteName = HostingEnvironment.SiteName;
                    var applicationId = HostingEnvironment.ApplicationID; // ie HttpRuntime.AppDomainAppId

                    var hashString = siteName + "::" + applicationId;
                    var hash = hashString.GenerateHash();
                    var siteTemp = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "UmbracoData", hash);

                    return _localTempPath = siteTemp;

                //case LocalTempStorage.Default:
                //case LocalTempStorage.Unknown:
                default:
                    return _localTempPath = ioHelper.MapPath("~/App_Data/TEMP");
            }

        }

    }
}
