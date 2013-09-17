/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using umbraco.DataLayer.Utility;
using umbraco.DataLayer.Utility.Installer;
using umbraco.DataLayer.Utility.Table;

namespace umbraco.DataLayer.SqlHelpers.SqlServer
{
    /// <summary>
    /// Utility for an SQL Server data source.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public class SqlServerUtility : DefaultUtility<SqlServerHelper>
    {
        #region Public Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerUtility"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        public SqlServerUtility(SqlServerHelper sqlHelper) : base(sqlHelper)
        { }

        #endregion

        #region DefaultUtility Members

        /// <summary>
        /// Creates an installer.
        /// </summary>
        /// <returns>The SQL Server installer.</returns>
        [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
        public override IInstallerUtility CreateInstaller()
        {
            return new SqlServerInstaller(SqlHelper);
        }

        /// <summary>
        /// Creates a table utility.
        /// </summary>
        /// <returns>The table utility</returns>
        public override ITableUtility CreateTableUtility()
        {
            return new SqlServerTableUtility(SqlHelper);
        }

        #endregion
    }
}
