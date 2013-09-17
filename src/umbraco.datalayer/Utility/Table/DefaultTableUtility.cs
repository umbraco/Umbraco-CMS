/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace umbraco.DataLayer.Utility.Table
{
    /// <summary>
    /// Default implementation of the <see cref="ITableUtility"/> interface.
    /// </summary>
    /// <typeparam name="S">The type of SQL helper.</typeparam>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public class DefaultTableUtility<S> : BaseUtility<S>, ITableUtility where S : ISqlHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTableUtility&lt;S&gt;"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        public DefaultTableUtility(S sqlHelper)
            : base(sqlHelper)
        { }

        #region ITableUtility Members

        /// <summary>
        /// Determines whether the table with the specified name exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the table exists; otherwise, <c>false</c>.</returns>
        public virtual bool ContainsTable(string name)
        {
            return GetTable(name) != null;
        }

        /// <summary>
        /// Gets the table with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The table, or <c>null</c> if no table with that name exists.</returns>
        public virtual ITable GetTable(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves or updates the table.
        /// </summary>
        /// <param name="table">The table to be saved.</param>
        public virtual void SaveTable(ITable table)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the table with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The table.</returns>
        public virtual ITable CreateTable(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (GetTable(name) != null)
                throw new ArgumentException(String.Format("A table named '{0}' already exists."), name);

            return new DefaultTable(name);
        }

        #endregion
    }
}
