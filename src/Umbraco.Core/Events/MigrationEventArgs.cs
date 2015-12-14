using System;
using System.Collections.Generic;
using System.ComponentModel;
using Semver;
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
        public MigrationEventArgs(IList<IMigration> eventObject, SemVersion configuredVersion, SemVersion targetVersion, bool canCancel)
            : base(eventObject, canCancel)
        {
            ConfiguredSemVersion = configuredVersion;
            TargetSemVersion = targetVersion;
        }

        [Obsolete("Use constructor accepting UmbracoVersion instances instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MigrationEventArgs(IList<IMigration> eventObject, Version configuredVersion, Version targetVersion, bool canCancel)
			: base(eventObject, canCancel)
         {
             ConfiguredSemVersion = new SemVersion(configuredVersion);
             TargetSemVersion = new SemVersion(targetVersion);
         }

        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="migrationContext"></param>
        /// <param name="targetVersion"></param>
        /// <param name="canCancel"></param>
        /// <param name="configuredVersion"></param>
        internal MigrationEventArgs(IList<IMigration> eventObject, MigrationContext migrationContext, SemVersion configuredVersion, SemVersion targetVersion, bool canCancel)
            : base(eventObject, canCancel)
        {
            MigrationContext = migrationContext;
            ConfiguredSemVersion = configuredVersion;
            TargetSemVersion = targetVersion;
        }

        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="configuredVersion"></param>
        /// <param name="targetVersion"></param>
        public MigrationEventArgs(IList<IMigration> eventObject, SemVersion configuredVersion, SemVersion targetVersion)
            : base(eventObject)
        {
            ConfiguredSemVersion = configuredVersion;
            TargetSemVersion = targetVersion;
        }

        [Obsolete("Use constructor accepting UmbracoVersion instances instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MigrationEventArgs(IList<IMigration> eventObject, Version configuredVersion, Version targetVersion)
			: base(eventObject)
		{
            ConfiguredSemVersion = new SemVersion(configuredVersion);
            TargetSemVersion = new SemVersion(targetVersion);
		}

		/// <summary>
		/// Returns all migrations that were used in the migration runner
		/// </summary>
        public IList<IMigration> Migrations
		{
			get { return EventObject; }
		}

        [Obsolete("Use ConfiguredSemVersion instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Version ConfiguredVersion
        {
            get { return ConfiguredSemVersion.GetVersion(); }
        }

        [Obsolete("Use TargetUmbracoVersion instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Version TargetVersion
        {
            get { return TargetSemVersion.GetVersion(); }
        }

        public SemVersion ConfiguredSemVersion { get; private set; }

        public SemVersion TargetSemVersion { get; private set; }

        internal MigrationContext MigrationContext { get; private set; }
    }
}