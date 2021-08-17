using System.Collections.Generic;

namespace Umbraco.Cms.Core.Notifications
{

    /// <summary>
    /// Published when unattended package migrations have been successfully executed
    /// </summary>
    public class UnattendedPackageMigrationsExecutedNotification : INotification
    {
        public UnattendedPackageMigrationsExecutedNotification(IReadOnlyList<string> packageMigrations)
            => PackageMigrations = packageMigrations;

        /// <summary>
        /// The list of package migration names that have been executed.
        /// </summary>
        public IReadOnlyList<string> PackageMigrations { get; }
    }
}
