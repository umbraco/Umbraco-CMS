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

namespace umbraco.DataLayer.SqlHelpers.MySql
{
    /// <summary>
    /// Utility for an MySql data source.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public class MySqlUtility : DefaultUtility<MySqlHelper>
    {
        #region Public Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlUtility"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        public MySqlUtility(MySqlHelper sqlHelper) : base(sqlHelper)
        { }

        #endregion

        #region DefaultUtility Members

        /// <summary>
        /// Creates an installer.
        /// </summary>
        /// <returns>The MySql installer.</returns>
        [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
        public override IInstallerUtility CreateInstaller()
        {
            return new MySqlInstaller(SqlHelper);
        }

        #endregion
    }
}
