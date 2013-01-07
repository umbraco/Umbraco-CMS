using System;
using Umbraco.Core.Configuration;
using umbraco.cms.businesslogic.installer;
using umbraco.IO;

namespace umbraco.presentation.install.steps.Definitions
{
    public class Database : InstallerStep
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
            get
            {
                return true;
            }
        }

        //here we determine if the installer should skip this step...
        public override bool Completed()
        {
            var configuredVersion = new Version(Umbraco.Core.Configuration.GlobalSettings.ConfigurationStatus);
            var targetVersion = UmbracoVersion.Current;
            
            return targetVersion < configuredVersion;
        }
    }
}