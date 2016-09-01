using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    /// <summary>
    /// This step is purely here to show the button to commence the upgrade
    /// </summary>
    [InstallSetupStep(InstallationType.Upgrade,
        "Upgrade", "upgrade", 1, "Upgrading Umbraco to the latest and greatest version.")]
    internal class UpgradeStep : InstallSetupStep<object>
    {
        public override bool RequiresExecution(object model)
        {
            return true;
        }

        public override InstallSetupResult Execute(object model)
        {
            return null;
        }

        public override object ViewModel
        {
            get
            {
                var currentVersion = CurrentVersion().GetVersion(3).ToString();
                var newVersion = UmbracoVersion.Current.ToString();
                var reportUrl = string.Format("https://our.umbraco.org/contribute/releases/compare?from={0}&to={1}&notes=1", currentVersion, newVersion);

                return new
                {
                    currentVersion = currentVersion,
                    newVersion = newVersion,
                    reportUrl = reportUrl
                };

            }
        }

        /// <summary>
        /// Gets the Current Version of the Umbraco Site before an upgrade
        /// by using the last/most recent Umbraco Migration that has been run
        /// </summary>
        /// <returns>A SemVersion of the latest Umbraco DB Migration run</returns>
        private SemVersion CurrentVersion()
        {
            //Set a default version of 0.0.0
            var version = new SemVersion(0);

            //If we have a db context available, if we don't then we are not installed anyways
            if (Current.DatabaseContext.IsDatabaseConfigured && Current.DatabaseContext.CanConnect)
            {
                version = Current.DatabaseContext.ValidateDatabaseSchema().DetermineInstalledVersionByMigrations(Current.Services.MigrationEntryService);
            }

            return version;
        }
        
    }
}