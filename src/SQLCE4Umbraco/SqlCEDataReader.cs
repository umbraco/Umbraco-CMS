/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System.Data.SqlServerCe;
using umbraco.DataLayer;

namespace SqlCE4Umbraco
{
    /// <summary>
    /// Class that adapts a SqlDataReader.SqlDataReader to a RecordsReaderAdapter.
    /// </summary>
    public class SqlCeDataReaderHelper : RecordsReaderAdapter<SqlCeDataReader>
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataReader"/> class.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        public SqlCeDataReaderHelper(System.Data.SqlServerCe.SqlCeDataReader dataReader) : base(dataReader) { }
                                                       
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
