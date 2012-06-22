/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace umbraco.DataLayer.SqlHelpers.SqlServer
{
    /// <summary>
    /// Parameter class for the SqlServerHelper.
    /// </summary>
    public class SqlServerParameter : SqlParameterAdapter<SqlParameter>
    {
        #region Public Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerParameter"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        public SqlServerParameter(string parameterName, object value)
            : base(new SqlParameter(parameterName, value))
        { }

        #endregion
    }
}
