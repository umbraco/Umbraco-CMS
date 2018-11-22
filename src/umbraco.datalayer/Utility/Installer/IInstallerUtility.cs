/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;

namespace umbraco.DataLayer.Utility.Installer
{
    /// <summary>
    /// Interface for a utility that helps installing an Umbraco data source.
    /// </summary>
    [Obsolete("This is not used and will be removed in future versions")]
    public interface IInstallerUtility
    {
        /// <summary>
        /// Gets the current data source version.
        /// </summary>
        /// <value>The current version.</value>
        DatabaseVersion CurrentVersion{ get; }

        /// <summary>
        /// Gets the latest available version.
        /// </summary>
        /// <value>The latest version.</value>
        DatabaseVersion LatestVersion{ get; }

        /// <summary>
        /// Gets a value indicating whether this installer can connect to the data source.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the installer can connect; otherwise, <c>false</c>.
        /// </value>
        bool CanConnect { get; }

        /// <summary>
        /// Gets a value indicating whether this installer can create a new database using the connection.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the installer can create a new database; otherwise, <c>false</c>.
        /// </value>
		//bool CanCreate { get; }

        /// <summary>
        /// Gets a value indicating whether the data source is empty and ready for installation.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the data source is empty; otherwise, <c>false</c>.
        /// </value>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets a value indicating whether the data source has an older version number.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the data source is older; otherwise, <c>false</c>.
        /// </value>
		//bool IsOlderVersion { get; }

        /// <summary>
        /// Gets a value indicating whether the data source has an up to date version.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the data source is up to date; otherwise, <c>false</c>.
        /// </value>
        bool IsLatestVersion { get; }

        /// <summary>
        /// Gets a value indicating whether the installer can upgrade the data source.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the installer can upgrade the data source; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Empty data sources can't be upgraded, just installed.</remarks>
        bool CanUpgrade { get; }

        /// <summary>
        /// Installs the latest version into the data source.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        /// If installing or upgrading is not supported.</exception>
        void Install();
    }
}
