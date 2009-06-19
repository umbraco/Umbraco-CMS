/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Resources;
using umbraco.DataLayer.Utility.Installer;
using System.Text.RegularExpressions;

namespace umbraco.DataLayer.SqlHelpers.MySql
{
    /// <summary>
    /// Database installer for an MySql data source.
    /// </summary>
    public class MySqlInstaller : DefaultInstallerUtility<MySqlHelper>
    {
        #region Private Constants

        /// <summary>The latest database version this installer supports.</summary>
        private const DatabaseVersion LatestVersionSupported = DatabaseVersion.Version4;

        /// <summary>The specifications to determine the database version.</summary>
        private static readonly VersionSpecs[] m_VersionSpecs = new VersionSpecs[] {
                    new VersionSpecs("action","umbracoAppTree",DatabaseVersion.Version4),
                    new VersionSpecs("version()","",DatabaseVersion.None) };

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether the installer can upgrade the data source.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the installer can upgrade the data source; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Empty data sources can't be upgraded, just installed.</remarks>
        public override bool CanUpgrade { get { return false; } }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the version specification for evaluation by DetermineCurrentVersion.
        /// Only first matching specification is taken into account.
        /// </summary>
        /// <value>The version specifications.</value>
        protected override VersionSpecs[] VersionSpecs { get { return m_VersionSpecs; } }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlInstaller"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        public MySqlInstaller(MySqlHelper sqlHelper)
            : base(sqlHelper, LatestVersionSupported)
        { }

        #endregion

        #region DefaultInstaller Members

        /// <summary>
        /// Installs the latest version into the data source.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        /// If installing or upgrading is not supported.</exception>
        public override void Install()
        {
            // Already installed?
            if (IsLatestVersion)
                return;
            // Only empty databases are supported right now.
            if (!IsEmpty)
                throw new NotSupportedException("Only empty databases are supported, upgrade not possible.");
            // Execute installation script.
            ExecuteStatements(GetMySqlVersion() >= 5 ? SqlResources.Total : ConvertToMySql4(SqlResources.Total));
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Converts the SQL command to the MySQL 4.x dialect.
        /// </summary>
        /// <param name="sql">The SQL in MySQL 5+ dialect.</param>
        /// <returns>The SQL in MySQL 4.x dialect.</returns>
        protected virtual string ConvertToMySql4(string sql)
        {
            // MySQL 4.x does not support the varchar datatype, but uses text instead
            return Regex.Replace(sql, @"(^?[ | n]varchar \(([2-9][5-9][6-9]|[0-9]{4,5})\)*)",
                                      @" text", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Gets the MySQL major version number.
        /// </summary>
        /// <returns>The MySQL major version number</returns>
        protected virtual int GetMySqlVersion()
        {
            return int.Parse(SqlHelper.ExecuteScalar<string>("SELECT SUBSTRING(VERSION(),1,1)"));
        }

        #endregion
    }
}
