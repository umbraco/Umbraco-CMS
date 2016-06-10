using System;
using System.Collections.Generic;
using System.ComponentModel;
using Semver;
using Umbraco.Core.Configuration;
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
        /// <param name="productName"></param>
        /// <param name="canCancel"></param>
        /// <param name="configuredVersion"></param>
        public MigrationEventArgs(IList<IMigration> eventObject, SemVersion configuredVersion, SemVersion targetVersion, string productName, bool canCancel)
            : this(eventObject, null, configuredVersion, targetVersion, productName, canCancel)
        { }

        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="targetVersion"></param>
        /// <param name="canCancel"></param>
        /// <param name="configuredVersion"></param>
        [Obsolete("Use constructor accepting a product name instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MigrationEventArgs(IList<IMigration> eventObject, SemVersion configuredVersion, SemVersion targetVersion, bool canCancel)
            : this(eventObject, null, configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName, canCancel)
        { }

        [Obsolete("Use constructor accepting SemVersion instances and a product name instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MigrationEventArgs(IList<IMigration> eventObject, Version configuredVersion, Version targetVersion, bool canCancel)
			: this(eventObject, null, new SemVersion(configuredVersion), new SemVersion(targetVersion), GlobalSettings.UmbracoMigrationName, canCancel)
         { }

        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="migrationContext"></param>
        /// <param name="targetVersion"></param>
        /// <param name="productName"></param>
        /// <param name="canCancel"></param>
        /// <param name="configuredVersion"></param>
        internal MigrationEventArgs(IList<IMigration> eventObject, MigrationContext migrationContext, SemVersion configuredVersion, SemVersion targetVersion, string productName, bool canCancel)
            : base(eventObject, canCancel)
        {
            MigrationContext = migrationContext;
            ConfiguredSemVersion = configuredVersion;
            TargetSemVersion = targetVersion;
            ProductName = productName;
        }

        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="migrationContext"></param>
        /// <param name="targetVersion"></param>
        /// <param name="canCancel"></param>
        /// <param name="configuredVersion"></param>
        [Obsolete("Use constructor accepting a product name instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal MigrationEventArgs(IList<IMigration> eventObject, MigrationContext migrationContext, SemVersion configuredVersion, SemVersion targetVersion, bool canCancel)
            : base(eventObject, canCancel)
        {
            MigrationContext = migrationContext;
            ConfiguredSemVersion = configuredVersion;
            TargetSemVersion = targetVersion;
            ProductName = GlobalSettings.UmbracoMigrationName;
        }

        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="configuredVersion"></param>
        /// <param name="targetVersion"></param>
        /// <param name="productName"></param>
        public MigrationEventArgs(IList<IMigration> eventObject, SemVersion configuredVersion, SemVersion targetVersion, string productName)
            : this(eventObject, null, configuredVersion, targetVersion, productName, false)
        { }

        /// <summary>
        /// Constructor accepting multiple migrations that are used in the migration runner
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="configuredVersion"></param>
        /// <param name="targetVersion"></param>
        [Obsolete("Use constructor accepting a product name instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MigrationEventArgs(IList<IMigration> eventObject, SemVersion configuredVersion, SemVersion targetVersion)
            : this(eventObject, null, configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName, false)
        { }

        [Obsolete("Use constructor accepting SemVersion instances and a product name instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MigrationEventArgs(IList<IMigration> eventObject, Version configuredVersion, Version targetVersion)
			: this(eventObject, null, new SemVersion(configuredVersion), new SemVersion(targetVersion), GlobalSettings.UmbracoMigrationName, false)
		{ }

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

        public string ProductName { get; private set; }

        internal MigrationContext MigrationContext { get; private set; }
    }
}