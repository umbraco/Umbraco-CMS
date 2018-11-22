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
    /// Version number of an Umbraco database.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public enum DatabaseVersion
    {
        /// <summary>Database connection unsuccessful.</summary>
        Unavailable,
        /// <summary>Empty database, connection successful.</summary>
        None,
        /// <summary>Umbraco version 3.0.</summary>
        Version3,
        /// <summary>Umbraco version 4.0.</summary>
        Version4,
		/// <summary>Umbraco version 4.1.</summary>
        Version4_1,
        /// <summary>Umbraco version 4.8.</summary>
        Version4_8
    }
}
