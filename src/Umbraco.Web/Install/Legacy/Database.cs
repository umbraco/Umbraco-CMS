using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Install.Steps
{
    internal class Database : InstallerStep
    {
        public override string Alias
        {
            get { return "database"; }
        }

        public override string Name
        {
            get { return "Database"; }
        }

        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/database.ascx"; }
        }


        public override bool MoveToNextStepAutomaticly
        {
            get { return true; }
        }

        //here we determine if the installer should skip this step...
        public override bool Completed()
        {

            // Fresh installs don't have a version number so this step cannot be complete yet
            if (string.IsNullOrEmpty(GlobalSettings.ConfigurationStatus))
            {
                //Even though the ConfigurationStatus is blank we try to determine the version if we can connect to the database
                var result = ApplicationContext.Current.DatabaseContext.ValidateDatabaseSchema();
                var determinedVersion = result.DetermineInstalledVersion();
                if (determinedVersion.Equals(new Version(0, 0, 0)))
                {
                    // Now that we know this is a brand new Umbraco install, turn membership providers' "useLegacyEncoding" off
                    DisableMembershipProviderLegacyEncoding();

                    return false;
                }
                return UmbracoVersion.Current < determinedVersion;
            }

            var configuredVersion = new Version(GlobalSettings.ConfigurationStatus);
            var targetVersion = UmbracoVersion.Current;

            return targetVersion < configuredVersion;
        }

        private static void DisableMembershipProviderLegacyEncoding()
        {
            if (GlobalSettings.UmbracoMembershipProviderLegacyEncoding)
                GlobalSettings.UmbracoMembershipProviderLegacyEncoding = false;
            
            if (GlobalSettings.UmbracoUsersMembershipProviderLegacyEncoding)
                GlobalSettings.UmbracoUsersMembershipProviderLegacyEncoding = false;
                
            
            LogHelper.Info<Database>("Updated Umbraco membership providers, set useLegacyEncoding to false.");
        }
    }
}