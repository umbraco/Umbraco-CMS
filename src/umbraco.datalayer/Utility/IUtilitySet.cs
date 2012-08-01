/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using umbraco.DataLayer.Utility.Installer;
using umbraco.DataLayer.Utility.Table;

namespace umbraco.DataLayer.Utility
{
    /// <summary>
    /// Interface for classes providing access to various Umbraco utilities
    /// that operate on data layer level.
    /// </summary>
    public interface IUtilitySet
    {
        /// <summary>
        /// Creates an installer.
        /// </summary>
        /// <returns>The installer.</returns>
        IInstallerUtility CreateInstaller();

        /// <summary>
        /// Creates a table utility.
        /// </summary>
        /// <returns>The table utility</returns>
        ITableUtility CreateTableUtility();
    }
}
