/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using MySqlClient = MySql.Data.MySqlClient;

namespace umbraco.DataLayer.SqlHelpers.MySql
{
    /// <summary>
    /// Class that adapts a MySql.Data.MySqlClient.MySqlDataReader to a RecordsReaderAdapter.
    /// </summary>
    public class MySqlDataReader : RecordsReaderAdapter<MySqlClient.MySqlDataReader>
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDataReader"/> class.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        public MySqlDataReader(MySqlClient.MySqlDataReader dataReader) : base(dataReader) { }

        #endregion

        #region RecordsReaderAdapter Members

        /// <summary>
        /// Gets a value indicating whether this instance has records.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has records; otherwise, <c>false</c>.
        /// </value>
        public override bool HasRecords
        {
            get { return DataReader.HasRows; }
        }

        #endregion
    }
}
