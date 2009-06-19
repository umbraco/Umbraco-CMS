/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Diagnostics;
using System.IO;

namespace umbraco.DataLayer.Extensions
{
    /// <summary>
    /// Extension to <see cref="ISqlHelper"/> that logs all executed commands to a stream.
    /// </summary>
    public class Logger : SqlHelperExtension
    {
        #region Private Constants
        
        /// <summary>Format of a line in the log file.</summary>
        private const string LogLineFormat = "#{0} ({1})\r\n{2}\r\n";
        
        #endregion

        #region Private Fields

        /// <summary>Stream to which the commands are logged.</summary>
        private readonly StreamWriter m_LogStream;

        /// <summary>Parser used to inline query parameters.</summary>
        private readonly SqlParser m_SqlParser = new SqlParser();

        #endregion

        #region Public Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="inputStream">The input stream to which the commands will be logged.</param>
        public Logger(Stream inputStream)
        {
            m_LogStream = new StreamWriter(inputStream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="filename">The name of the file in which the commands will be logged.</param>
        public Logger(string filename)
        {
            m_LogStream = new StreamWriter(filename, true);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Logger"/> is reclaimed by garbage collection.
        /// </summary>
        ~Logger()
        {
            try
            {
                m_LogStream.Close();
            }
            catch { }
        }

        #endregion

        #region SqlHelperExtension Members

        /// <summary>
        /// Called when ExecuteScalar is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        public override void OnExecuteScalar(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters)
        {
            AppendToLog(sqlHelper, commandText, parameters);
        }

        /// <summary>
        /// Called when ExecuteNonQuery is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        public override void OnExecuteNonQuery(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters)
        {
            AppendToLog(sqlHelper, commandText, parameters);
        }

        /// <summary>
        /// Called when ExecuteReader is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        public override void OnExecuteReader(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters)
        {
            AppendToLog(sqlHelper, commandText, parameters);
        }

        /// <summary>
        /// Called when ExecuteXmlReader is executed.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The SQL parameters.</param>
        public override void OnExecuteXmlReader(ISqlHelper sqlHelper, ref string commandText, ref IParameter[] parameters)
        {
            AppendToLog(sqlHelper, commandText, parameters);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Appends a command to the log stream.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper executing the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        private void AppendToLog(ISqlHelper sqlHelper, string commandText, IParameter[] parameters)
        {
            m_LogStream.WriteLine(LogLineFormat,
                                          DateTime.Now,
                                          new StackFrame(1).GetMethod().Name,
                                          m_SqlParser.ApplyParameters(commandText, parameters));
            m_LogStream.Flush();
        }

        #endregion
    }
}
