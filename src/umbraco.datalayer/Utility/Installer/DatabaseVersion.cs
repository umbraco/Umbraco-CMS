/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

namespace umbraco.DataLayer.Utility.Installer
{
    /// <summary>
    /// Version number of an Umbraco database.
    /// </summary>
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
