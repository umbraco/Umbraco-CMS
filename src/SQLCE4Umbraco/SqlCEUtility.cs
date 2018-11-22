/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using umbraco.DataLayer.SqlHelpers.SqlServer;
using umbraco.DataLayer.Utility;
using umbraco.DataLayer.Utility.Installer;
using umbraco.DataLayer.Utility.Table;

namespace SqlCE4Umbraco
{
    /// <summary>
    /// Utility for an SQL Server data source.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public class SqlCEUtility : DefaultUtility<SqlCEHelper>
    {
        #region Public Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerUtility"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        public SqlCEUtility(SqlCEHelper sqlHelper) : base(sqlHelper)
        { }

        #endregion

        #region DefaultUtility Members

        /// <summary>
        /// Creates an installer.
        /// </summary>
        /// <returns>The SQL Server installer.</returns>
        public override IInstallerUtility CreateInstaller()
        {
            return new SqlCEInstaller(SqlHelper);
        }

        /// <summary>
        /// Creates a table utility.
        /// </summary>
        /// <returns>The table utility</returns>
        public override ITableUtility CreateTableUtility()
        {
            return new SqlCETableUtility(SqlHelper);
        }

        #endregion
    }
}
