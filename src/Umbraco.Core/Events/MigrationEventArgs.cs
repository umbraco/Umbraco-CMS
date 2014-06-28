using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Core.Events
{
    public class MigrationEventArgs : CancellableObjectEventArgs<IList<IMigration>>
    {
        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="targetVersion"></param>
        /// <param name="canCancel"></param>
        /// <param name="configuredVersion"></param>
        public MigrationEventArgs(IList<IMigration> eventObject, Version configuredVersion, Version targetVersion, bool canCancel)
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
        internal MigrationEventArgs(IList<IMigration> eventObject, MigrationContext migrationContext, Version configuredVersion, Version targetVersion, bool canCancel)
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
        public MigrationEventArgs(IList<IMigration> eventObject, Version configuredVersion, Version targetVersion)
			: base(eventObject)
		{
            ConfiguredVersion = configuredVersion;
            TargetVersion = targetVersion;
		}

		/// <summary>
		/// Returns all migrations that were used in the migration runner
		/// </summary>
        public IList<IMigration> Migrations
		{
			get { return EventObject; }
		}

        public Version ConfiguredVersion { get; private set; }

        public Version TargetVersion { get; private set; }

        internal MigrationContext MigrationContext { get; private set; }
    }
}