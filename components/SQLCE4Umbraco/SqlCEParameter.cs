/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Data.SqlServerCe;
using System.Data.SqlTypes;
using umbraco.DataLayer;

namespace SqlCE4Umbraco
{
    /// <summary>
    /// Parameter class for the SqlCEHelper.
    /// </summary>
    public class SqlCEParameter : SqlParameterAdapter<SqlCeParameter>
    {
        #region Public Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCEParameter"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        public SqlCEParameter(string parameterName, object value)
            : base(new SqlCeParameter(parameterName, value))
        { }

        #endregion
    }
}
