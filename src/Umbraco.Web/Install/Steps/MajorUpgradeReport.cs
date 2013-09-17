using System;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using GlobalSettings = umbraco.GlobalSettings;

namespace Umbraco.Web.Install.Steps
{
    /// <summary>
    /// An installer step that is shown when detecting that a major upgrade is about to occur
    /// </summary>
    internal class MajorUpgradeReport : InstallerStep
    {
        private bool _moveToNextStepAutomaticly;

        public MajorUpgradeReport()
        {
            _moveToNextStepAutomaticly = true;
        }

        public override string Alias
        {
            get { return "majorUpgrade"; }
        }

        public override string Name
        {
            get { return "Upgrade report"; }
        }

        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/upgradereport.ascx"; }
        }

        /// <summary>
        /// Return false if it is an upgrade and the upgrade is for a major version otherwise return true and do not show this step
        /// </summary>
        /// <returns></returns>
        public override bool Completed()
        {
            //we cannot run this step if the db is not configured.
            if (ApplicationContext.Current.DatabaseContext.IsDatabaseConfigured == false)
            {
                return true;
            }

            var result = ApplicationContext.Current.DatabaseContext.ValidateDatabaseSchema();
            var determinedVersion = result.DetermineInstalledVersion();
            if ((string.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus) == false || determinedVersion.Equals(new Version(0, 0, 0)) == false)
                && UmbracoVersion.Current.Major > determinedVersion.Major)
            {
                //it's an upgrade to a major version so we're gonna show this step
                return false;
            }
            return true;
        }

        public override bool HideFromNavigation
        {
            get { return true; }
        }

        public override bool MoveToNextStepAutomaticly
        {
            get { return _moveToNextStepAutomaticly; }
            set { _moveToNextStepAutomaticly = value; }
        }
    }
}