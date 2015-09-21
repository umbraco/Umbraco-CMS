using Umbraco.Core;
using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// Base class that can be used to run code after the migration runner has executed
    /// </summary>
    public abstract class MigrationStartupHander : ApplicationEventHandler
    {
        /// <summary>
        /// Ensure this is run when not configured
        /// </summary>
        protected override bool ExecuteWhenApplicationNotConfigured
        {
            get { return true; }
        }

        /// <summary>
        /// Ensure this is run when not configured
        /// </summary>
        protected override bool ExecuteWhenDatabaseNotConfigured
        {
            get { return true; }
        }

        public void Unsubscribe()
        {
            MigrationRunner.Migrated -= MigrationRunner_Migrated;
        }

        /// <summary>
        /// Attach to event on starting
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            MigrationRunner.Migrated += MigrationRunner_Migrated;
        }

        private void MigrationRunner_Migrated(MigrationRunner sender, Core.Events.MigrationEventArgs e)
        {
            AfterMigration(sender, e);
        }

        protected abstract void AfterMigration(MigrationRunner sender, Core.Events.MigrationEventArgs e);
    }
}