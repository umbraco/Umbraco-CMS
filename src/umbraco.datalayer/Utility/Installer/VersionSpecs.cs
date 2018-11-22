using System;

namespace umbraco.DataLayer.Utility.Installer
{
    /// <summary>
    /// A triple (Field, Table, Version) meaning:
    /// if a <c>SELECT</c> statement of <c>Field FROM Table</c> succeeds,
    /// the database version is at least <c>Version</c>.
    /// </summary>
    /// <remarks>
    /// This also supports checking for a value in a table.
    /// </remarks>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public struct VersionSpecs
    {
        /// <summary>The SQL statament to execute in order to test for the specified version</summary>
        public readonly string Sql;

        /// <summary>An integer identifying the expected row count from the Sql statement</summary>
        public readonly int ExpectedRows;

        /// <summary>The minimum version number of a database that contains the specified field.</summary>
        public readonly DatabaseVersion Version;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionSpecs"/> struct.
        /// </summary>
        /// <param name="sql">The sql statement to execute.</param>
        /// <param name="version">The version.</param>
        public VersionSpecs(string sql, DatabaseVersion version)
            : this(sql, -1, version)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionSpecs"/> struct.
        /// </summary>
        /// <param name="sql">The sql statement to execute.</param>
        /// <param name="expectedRows">The expected row count.</param>
        /// <param name="version">The version.</param>
        public VersionSpecs(string sql, int expectedRows, DatabaseVersion version)
        {
            Sql = sql;
            ExpectedRows = expectedRows;
            Version = version;
        }
    }
}