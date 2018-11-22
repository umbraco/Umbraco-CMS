/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;

namespace umbraco.DataLayer.Utility
{
    /// <summary>
    /// Base class for utilities that use an ISqlHelper as data source.
    /// </summary>
    /// <typeparam name="S">The SQL helper type.</typeparam>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public abstract class BaseUtility<S> where S : ISqlHelper
    {
        #region Private Fields

        /// <summary>SQL helper the utility uses as data source.</summary>
        private readonly S m_SqlHelper;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected S SqlHelper
        {
            get { return m_SqlHelper; }
        }

        #endregion

        #region Protected Constructors
        
        /// <summary>
        /// Initializes the <see cref="BaseUtility&lt;S&gt;"/> with the specified SQL helper.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        protected BaseUtility(S sqlHelper)
        {
            m_SqlHelper = sqlHelper;
        }

        #endregion
    }
}
