/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;

namespace umbraco.DataLayer.Utility.Table
{
    /// <summary>
    /// Interface for a tool that provides access to the tables of a data source.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public interface ITableUtility
    {
        /// <summary>
        /// Determines whether the table with the specified name exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the table exists; otherwise, <c>false</c>.</returns>
        bool ContainsTable(string name);

        /// <summary>
        /// Gets the table with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The table, or <c>null</c> if no table with that name exists.</returns>
        ITable GetTable(string name);

        /// <summary>
        /// Creates the table with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The table.</returns>
        ITable CreateTable(string name);

        /// <summary>
        /// Saves or updates the table.
        /// </summary>
        /// <param name="table">The table to be saved.</param>
        void SaveTable(ITable table);
    }
}
