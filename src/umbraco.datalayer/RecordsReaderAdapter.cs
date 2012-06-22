/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace umbraco.DataLayer
{
    /// <summary>
    /// Class that adapts a generic data reader to an IRecordsReader.
    /// </summary>
    /// <typeparam name="D">The data reader class</typeparam>
    public abstract class RecordsReaderAdapter<D> : IRecordsReader
                                            where D : IDataReader, IDataRecord, IEnumerable
    {
        #region Private Fields

        /// <summary> Wrapped data reader. </summary>
        private readonly D m_DataReader;

        /* Use the DebugDataLayer compile constant to get information about the opened RecordsReaderAdapters. */
        #if DEBUG && DebugDataLayer

            /// <summary>Application unique identifier of this RecordsReaderAdapter.</summary>
            /// <remarks>Used to track different instantiations for debug purposes.</remarks>
            private readonly int m_Id;

            /// <summary>Application unique identifier that will be assigned to the next new RecordsReaderAdapter.</summary>
            /// <remarks>Used to track different instantiations for debug purposes.</remarks>
            private static int m_GlobalId = 0;

            /// <summary>List of identifiers of open RecordsReaderAdapters.</summary>
            /// <remarks>Used to track different instantiations for debug purposes.</remarks>
            private static List<int> m_OpenDataReaderIds = new List<int>();

        #endif

        #endregion

        #region Public Properties
        /// <summary>Gets the internal data reader.</summary>
        /// <value>The data reader.</value>
        public D DataReader
        {
            get { return m_DataReader; }
        }

        /// <summary>Gets the internal data reader.</summary>
        /// <value>The data reader.</value>
        /// <remarks>Obsolete. You should NOT try to close or dispose the RawDataReader,
        ///          but instead close or dispose this RecordsReaderAdapter.
        ///          Inheriting classes can call the protected DataReader property.</remarks>
        [Obsolete("Only for backwards compatibility.", false)]
        public D RawDataReader
        {
            get { return m_DataReader; }
        }

        #endregion

        #region Constructors and destructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordsReaderAdapter&lt;D&gt;"/> class.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        public RecordsReaderAdapter(D dataReader)
        {
            m_DataReader = dataReader;

            #if DEBUG && DebugDataLayer
                lock (m_OpenDataReaderIds)
                {
                    // Get application unique identifier
                    m_Id = m_GlobalId++;
                    // Signal the creation of this new RecordsReaderAdapter
                    m_OpenDataReaderIds.Add(m_Id);
                    StackFrame stackFrame = new StackFrame(4);
                    string caller = stackFrame.GetMethod().ReflectedType.Name + "." + stackFrame.GetMethod().Name;
                    Trace.TraceInformation(m_Id + ". RecordsReader created by " + caller + ". "
                                               + "Open Data Readers: " + m_OpenDataReaderIds.Count);
                }
            #endif
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="RecordsReaderAdapter&lt;D&gt;"/> is reclaimed by garbage collection.
        /// </summary>
        ~RecordsReaderAdapter()
        {
            Dispose(false);
        }
        #endregion

        #region IRecordsReader Members

        /// <summary>
        /// Closes the data reader.
        /// </summary>
        public virtual void Close()
        {
            try
            {
                m_DataReader.Close();
            }
            catch { }

            #if DEBUG && DebugDataLayer
                // Log closing
                lock (m_OpenDataReaderIds)
                {
                    m_OpenDataReaderIds.Remove(m_Id);
                    Trace.TraceInformation(m_Id + ". RecordsReader closed. "
                                               + "Open Data Readers: " + m_OpenDataReaderIds.Count);
                }
            #endif
        }

        /// <summary>
        /// Gets the depth of nesting of the current row.
        /// </summary>
        /// <value>The depth of nesting of the current row.</value>
        public int Depth
        {
            get { return m_DataReader.Depth; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is closed.
        /// </summary>
        /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
        public bool IsClosed
        {
            get { return m_DataReader.IsClosed; }
        }

        /// <summary>
        /// Advances to the next record.
        /// </summary>
        /// <returns>
        /// <c>true</c> if there are more records; otherwise, <c>false</c>.
        /// </returns>
        public bool Read()
        {
            return m_DataReader.Read();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has records.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has records; otherwise, <c>false</c>.
        /// </value>
        public abstract bool HasRecords { get; }

        /// <summary>
        /// Gets the value of the specified field.
        /// </summary>
        /// <typeparam name="FieldType">The field type.</typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the specified field</returns>
        public FieldType Get<FieldType>(string fieldName)
        {
            switch (typeof(FieldType).FullName)
            {
                case "System.Boolean":  return (FieldType)(object)GetBoolean(fieldName);
                case "System.Byte":     return (FieldType)(object)GetByte(fieldName);
                case "System.DateTime": return (FieldType)(object)GetDateTime(fieldName);
                case "System.Decimal":  return (FieldType)(object)GetDecimal(fieldName);
                case "System.Double":   return (FieldType)(object)GetDouble(fieldName);
                case "System.Single":   return (FieldType)(object)GetFloat(fieldName);
                case "System.Guid":     return (FieldType)(object)GetGuid(fieldName);
                case "System.Int16":    return (FieldType)(object)GetShort(fieldName);
                case "System.Int32":    return (FieldType)(object)GetInt(fieldName);
                case "System.Int64":    return (FieldType)(object)GetLong(fieldName);
                case "System.String":   return (FieldType)(object)GetString(fieldName);
                default:                return (FieldType)GetObject(fieldName);
            }
        }

        /// <summary>
        /// Gets the value of the specified field as a bool.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public bool GetBoolean(string fieldName)
        {
            return m_DataReader.GetBoolean(GetOrdinal(fieldName));
        }

        /// <summary>
        /// Gets the value of the specified field as a byte.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public byte GetByte(string fieldName)
        {
            // this is needed as SQL CE 4 doesn't support smallint/tinyint as IDENTITY columns. So some columns that are
            // int16 in SQL Server (smallint/tinyint) will be int32 (int) in SQL CE 4
            int fieldNo = GetOrdinal(fieldName);
            Type t = m_DataReader.GetFieldType(fieldNo);
            if (t.FullName.ToLower() == "system.int32") // SQL CE4 behavior
                return Byte.Parse(m_DataReader.GetInt32(fieldNo).ToString());

            return m_DataReader.GetByte(fieldNo);
        }

        /// <summary>
        /// Gets the value of the specified field as a DateTime.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public DateTime GetDateTime(string fieldName)
        {
            return m_DataReader.GetDateTime(GetOrdinal(fieldName));
        }

        /// <summary>
        /// Gets the value of the specified field as a decimal.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public decimal GetDecimal(string fieldName)
        {
            return m_DataReader.GetDecimal(GetOrdinal(fieldName));
        }

        /// <summary>
        /// Gets the value of the specified field as a double.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public double GetDouble(string fieldName)
        {
            return m_DataReader.GetDouble(GetOrdinal(fieldName));
        }

        /// <summary>
        /// Gets the value of the specified field as a float.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public float GetFloat(string fieldName)
        {
            return m_DataReader.GetFloat(GetOrdinal(fieldName));
        }

        /// <summary>
        /// Gets the value of the specified field as a Guid.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public Guid GetGuid(string fieldName)
        {
            return m_DataReader.GetGuid(GetOrdinal(fieldName));
        }

        /// <summary>
        /// Gets the value of the specified field as a short.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public short GetShort(string fieldName)
        {
            // this is needed as SQL CE 4 doesn't support smallint/tinyint as IDENTITY columns. So some columns that are
            // int16 in SQL Server (smallint/tinyint) will be int32 (int) in SQL CE 4
            int fieldNo = GetOrdinal(fieldName);
            Type t = m_DataReader.GetFieldType(fieldNo);
            if (t.FullName == "System.Int32")
                return (short) m_DataReader.GetInt32(fieldNo);

            return m_DataReader.GetInt16(fieldNo);
        }

        /// <summary>
        /// Gets the value of the specified field as an int.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public int GetInt(string fieldName)
        {
            int ordinal = GetOrdinal(fieldName);
            return m_DataReader.IsDBNull(ordinal) ? -1 : m_DataReader.GetInt32(ordinal);
        }

        /// <summary>
        /// Gets the value of the specified field as a long.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public long GetLong(string fieldName)
        {
            return m_DataReader.GetInt64(GetOrdinal(fieldName));
        }

        /// <summary>
        /// Gets the value of the specified field as a string.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public string GetString(string fieldName)
        {
            int ordinal = GetOrdinal(fieldName);
            return m_DataReader.IsDBNull(ordinal) ? null : m_DataReader.GetString(ordinal);
        }

        /// <summary>
        /// Gets the value of the specified field.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        public object GetObject(string fieldName)
        {
            int ordinal = GetOrdinal(fieldName);
            return m_DataReader.IsDBNull(ordinal) ? null : m_DataReader.GetValue(ordinal);
        }

        /// <summary>
        /// Determines whether the specified field is null.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// 	<c>true</c> if the specified field is null; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNull(string fieldName)
        {
            return m_DataReader.IsDBNull(GetOrdinal(fieldName));
        }

        /// <summary>
        /// Determines whether a field with the specified field name exists in the record.
        /// The field can still contain a null value.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// 	<c>true</c> if the specified field exists; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsField(string fieldName)
        {
            bool fieldExists;
            try
            {
                GetOrdinal(fieldName);
                fieldExists = true;
            }
            catch
            {
                // GetOrdinal failed, field was not found.
                fieldExists = false;
            }
            return fieldExists;
        }

        /// <summary>
        /// Returns the index of the field with the specified name.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The index of the field.</returns>
        protected virtual int GetOrdinal(string fieldName)
        {
            return m_DataReader.GetOrdinal(fieldName);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the records.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through records.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return m_DataReader.GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        ///                         <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Try to close the data reader if it's still open
            try
            {
                if (m_DataReader != null && !m_DataReader.IsClosed)
                    Close();
            }
            finally
            {
                // Try to dispose the data reader
                try
                {
                    if (m_DataReader != null)
                        m_DataReader.Dispose();
                }
                catch { }
                // Dispose methods should call SuppressFinalize
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        #endregion
    }
}
