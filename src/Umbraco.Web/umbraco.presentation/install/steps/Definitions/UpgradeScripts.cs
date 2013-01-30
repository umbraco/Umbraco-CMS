using System;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Install.UpgradeScripts;
using umbraco.DataLayer.Utility.Installer;
using umbraco.cms.businesslogic.installer;

namespace umbraco.presentation.install.steps.Definitions
{
    internal class UpgradeScripts : InstallerStep
    {

        

        public override string Alias
        {
            get { return "upgradeScripts"; }
        }

        public override bool HideFromNavigation
        {
            get { return true; }
        }

        /// <summary>
        /// If there are no scripts for this version the skip
        /// </summary>
        /// <returns></returns>
        public override bool Completed()
        {
            var canConnect = CanConnectToDb();
            //if we cannot connect to the db, then we cannot run the script and most likely the database doesn't exist yet anyways.
            if (!canConnect) return true; //skip

            //if the version is empty then it's probably a new installation, we cannot run scripts
            if (GlobalSettings.CurrentVersion.IsNullOrWhiteSpace()) return true; //skip
            var currentUmbVersion = Umbraco.Core.Configuration.GlobalSettings.GetConfigurationVersion();
            if (currentUmbVersion == null)
                return true; //skip, could not get a version

            //check if we have upgrade script to run for this version
            var hasScripts = UpgradeScriptManager.HasScriptsForVersion(currentUmbVersion);
            return !hasScripts;
        }

        public override string Name
        {
            get { return "Upgrade scripts"; }
        }

        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/UpgradeScripts.ascx"; }
        }

        public override bool MoveToNextStepAutomaticly
        {
            get
            {
                return true;
            }
        }

        private bool CanConnectToDb()
        {
            try
            {
                var installer = BusinessLogic.Application.SqlHelper.Utility.CreateInstaller();
                var latest =  installer.IsLatestVersion;
                return true; //if we got this far, we can connect.
            }
            catch
            {                
                return false;
            }
        }
    }
}