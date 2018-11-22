/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

namespace umbraco.DataLayer.Extensions
{
    /// <summary>
    /// Abstract base class for an SQL helper extension.
    /// Implements all methods with an empty body.
    /// You will find it convenient to use this class as a base for most extensions,
    /// because you only have to override the functions you actually use.
    /// </summary>
    public abstract class SqlHelperExtension : ISqlHelperExtension
    {
        #region ISqlHelperExtension Members

        /// <summary>
        /// Called when ExecuteScalar is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        public virtual void OnExecuteScalar(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters)
        {}

        /// <summary>
        /// Called when ExecuteNonQuery is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        public virtual void OnExecuteNonQuery(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters)
        {}

        /// <summary>
        /// Called when ExecuteReader is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        public virtual void OnExecuteReader(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters)
        {}

        /// <summary>
        /// Called when ExecuteXmlReader is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        public virtual void OnExecuteXmlReader(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters)
        {}

        #endregion
    }
}
