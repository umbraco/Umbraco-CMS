using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Core.Events
{
    public class MigrationEventArgs : CancellableObjectEventArgs<IEnumerable<IMigration>>
    {
        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="targetVersion"></param>
        /// <param name="canCancel"></param>
        /// <param name="configuredVersion"></param>
        public MigrationEventArgs(IEnumerable<IMigration> eventObject, Version configuredVersion, Version targetVersion, bool canCancel)
			: base(eventObject, canCancel)
         {
             ConfiguredVersion = configuredVersion;
             TargetVersion = targetVersion;
         }

        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="migrationContext"></param>
        /// <param name="targetVersion"></param>
        /// <param name="canCancel"></param>
        /// <param name="configuredVersion"></param>
        internal MigrationEventArgs(IEnumerable<IMigration> eventObject, MigrationContext migrationContext, Version configuredVersion, Version targetVersion, bool canCancel)
            : base(eventObject, canCancel)
        {
            MigrationContext = migrationContext;
            ConfiguredVersion = configuredVersion;
            TargetVersion = targetVersion;
        }

        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="configuredVersion"></param>
        /// <param name="targetVersion"></param>
        public MigrationEventArgs(IEnumerable<IMigration> eventObject, Version configuredVersion, Version targetVersion)
			: base(eventObject)
		{
            ConfiguredVersion = configuredVersion;
            TargetVersion = targetVersion;
		}

		/// <summary>
		/// Returns all migrations that were used in the migration runner
		/// </summary>
        public IEnumerable<IMigration> Migrations
		{
			get { return EventObject; }
		}

        public Version ConfiguredVersion { get; private set; }

        public Version TargetVersion { get; private set; }

        internal MigrationContext MigrationContext { get; private set; }
    }
}