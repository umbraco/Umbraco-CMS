/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using MySqlClient = MySql.Data.MySqlClient;
using System.Text;

namespace umbraco.DataLayer.SqlHelpers.MySql
{
    /// <summary>
    /// Parameter class for the MySqlHelper.
    /// </summary>
    public class MySqlParameter : SqlParameterAdapter<MySqlClient.MySqlParameter>
    {
        #region Public Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlParameter"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        public MySqlParameter(string parameterName, object value)
            : base(new MySqlClient.MySqlParameter(CorrectParameterName(parameterName), value))
        { }

        #endregion

        #region Private Methods

        /// <summary>
        /// Changes the parameter name to the MySQL equivalent.
        /// </summary>
        /// <param name="parameterName">The name of the parameter, preceded by an at sign.</param>
        /// <returns>The name of the parameter, preceded by a question mark.</returns>
        private static string CorrectParameterName(string parameterName)
        {
            // check parameters
            if(String.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException("parameterName");

            // do nothing if parameter doesn't begin with at sign
            if (parameterName[0] != '@')
                return parameterName;

            // replace the at sign by a question mark
            StringBuilder sb = new StringBuilder(parameterName);
            sb[0] = '?';
            return sb.ToString();
        }

        #endregion
    }
}
