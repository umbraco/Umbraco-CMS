/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System.Data;
using System.Xml;

namespace umbraco.DataLayer.Extensions
{
    /// <summary>
    /// Interface for an extension to an <see cref="ISqlHelper"/>.
    /// </summary>
    public interface ISqlHelperExtension
    {
        /// <summary>
        /// Called when <c>ExecuteScalar</c> is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        void OnExecuteScalar(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters);

        /// <summary>
        /// Called when <c>ExecuteNonQuery</c> is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        void OnExecuteNonQuery(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters);

        /// <summary>
        /// Called when <c>ExecuteReader</c> is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        void OnExecuteReader(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters);

        /// <summary>
        /// Called when <c>ExecuteXmlReader</c> is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        void OnExecuteXmlReader(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters);
    }
}
