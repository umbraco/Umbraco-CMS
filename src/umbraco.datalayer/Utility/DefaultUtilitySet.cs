/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using umbraco.DataLayer.Utility.Installer;
using umbraco.DataLayer.Utility.Table;
using System;

namespace umbraco.DataLayer.Utility
{
    /// <summary>
    /// Base class providing access to various Umbraco utilities
    /// that operate on data layer level.
    /// </summary>
    /// <typeparam name="S"></typeparam>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public class DefaultUtility<S> : IUtilitySet where S : ISqlHelper
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

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUtility&lt;S&gt;"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        public DefaultUtility(S sqlHelper)
        {
            m_SqlHelper = sqlHelper;
        }

        #endregion
       
        #region IUmbracoUtility Members

        /// <summary>
        /// Creates an installer.
        /// </summary>
        /// <returns>The default installer.</returns>
        public virtual IInstallerUtility CreateInstaller()
        {
			throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a table utility.
        /// </summary>
        /// <returns>The table utility</returns>
        public virtual ITableUtility CreateTableUtility()
        {
            return new DefaultTableUtility<S>(SqlHelper);
        }

        #endregion
    }
}
