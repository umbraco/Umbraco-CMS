using System;
using System.Linq;
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
                version = Current.DatabaseContext.ValidateDatabaseSchema().DetermineInstalledVersionByMigrations(Current.Services.MigrationEntryService);

            if (version != new SemVersion(0))
                return version;

            // If we aren't able to get a result from the umbracoMigrations table then use the version in web.config, if it's available
            if (string.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus))
                return version;

            var configuredVersion = GlobalSettings.ConfigurationStatus;

            string currentComment = null;

            var current = configuredVersion.Split('-');
            if (current.Length > 1)
                currentComment = current[1];

            Version currentVersion;
            if (Version.TryParse(current[0], out currentVersion))
            {
                version = new SemVersion(
                    currentVersion.Major,
                    currentVersion.Minor,
                    currentVersion.Build,
                    string.IsNullOrWhiteSpace(currentComment) ? null : currentComment,
                    currentVersion.Revision > 0 ? currentVersion.Revision.ToString() : null);
            }

            return version;
        }
    }
}