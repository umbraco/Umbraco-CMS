/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using umbraco.DataLayer.Utility.Installer;
using umbraco.DataLayer.Utility.Table;

namespace umbraco.DataLayer.Utility
{
    /// <summary>
    /// Interface for classes providing access to various Umbraco utilities
    /// that operate on data layer level.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public interface IUtilitySet
    {
        /// <summary>
        /// Creates an installer.
        /// </summary>
        /// <returns>The installer.</returns>
        [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
        IInstallerUtility CreateInstaller();

        /// <summary>
        /// Creates a table utility.
        /// </summary>
        /// <returns>The table utility</returns>
        [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
        ITableUtility CreateTableUtility();
    }
}
