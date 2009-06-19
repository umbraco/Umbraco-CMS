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

namespace umbraco.DataLayer
{
    /// <summary>
    /// Represents an object that reads record data from a result set.
    /// </summary>
    public interface IRecordsReader : IEnumerable, IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance has records.
        /// </summary>
        /// <value><c>true</c> if this instance has records; otherwise, <c>false</c>.</value>
        bool HasRecords { get; }

        #endregion

        #region Reading Methods

        /// <summary>
        /// Advances to the next record.
        /// </summary>
        /// <returns>
        /// <c>true</c> if there are more records; otherwise, <c>false</c>.
        /// </returns>
        bool Read();

        /// <summary>
        /// Closes the reader.
        /// </summary>
        void Close();
        #endregion

        #region Field Getters

        /// <summary>
        /// Determines whether a field with the specified field name exists in the record.
        /// The field can still contain a null value.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns><c>true</c> if the specified field exists; otherwise, <c>false</c>.</returns>
        bool ContainsField(string fieldName);

        /// <summary>
        /// Determines whether the specified field is null.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns><c>true</c> if the specified field is null; otherwise, <c>false</c>.</returns>
        bool IsNull(string fieldName);

        /// <summary>
        /// Gets the specified field value.
        /// </summary>
        /// <typeparam name="FieldType">The field type.</typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        FieldType Get<FieldType>(string fieldName);

        /// <summary>
        /// Gets the value of the specified field.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        object GetObject(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as a bool.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        bool GetBoolean(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as a byte.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        byte GetByte(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as a DateTime.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        DateTime GetDateTime(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as a decimal.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        decimal GetDecimal(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as a double.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        double GetDouble(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as a float.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        float GetFloat(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as a Guid.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        Guid GetGuid(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as a short.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        short GetShort(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as an int.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        int GetInt(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as a long.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        long GetLong(string fieldName);

        /// <summary>
        /// Gets the value of the specified field as a string.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="System.IndexOutOfRangeException">No column with the specified name was found.</exception>
        string GetString(string fieldName);

        #endregion
    }
}
