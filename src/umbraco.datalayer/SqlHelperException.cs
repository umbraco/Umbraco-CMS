/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.DataLayer
{
    /// <summary>
    /// Exception generated in an SQL helper.
    /// </summary>
    public class SqlHelperException : UmbracoException
    {
        #region Private Fields

        /*
         * Make command text and parameters available when in debug mode.
         * (General debug mode, not only data layer debug mode: exceptions that occur in commands
         * are most likely not due to errors in the data layer itself, but in the calling method.)
         */
        #if DEBUG

            /// <summary> The text of the command that resulted in the exception.</summary>
            private readonly string m_CommandText;
            /// <summary> The parameters of the command that resulted in the exception.</summary>
            private readonly IParameter[] m_Parameters;

        #endif // DEBUG

        #endregion

        #region Public Properties

            /*
             * Make command text and parameters available when in debug mode.
             * (General debug mode, not only data layer debug mode: exceptions that occur in commands
             * are most likely not due to errors in the data layer itself, but in the calling method.)
             */
            #if DEBUG

                /// <summary>
                /// Returns the text of the command that resulted in the exception.
                /// </summary>
                /// <value>The command text.</value>
                /// <remarks>Attention! Use for debugging purposes only.</remarks>
                public string CommandText
                {
                    get { return m_CommandText; }
                }

                /// <summary>
                /// Gets the parameters of the command that resulted in the exception.
                /// </summary>
                /// <value>The command parameters.</value>
                /// <remarks>Attention! Use for debugging purposes only.</remarks>
                public IParameter[] Parameters
                {
                    get { return m_Parameters; }
                }

            #endif // DEBUG 

            #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlHelperException"/> class.
        /// </summary>
        /// <param name="method">The method where the exception occurred.</param>
        /// <param name="commandText">The command text. Only used in debug mode.</param>
        /// <param name="parameters">The command parameters. Only used in debug mode.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <remarks>
        /// *Never* place the command text inside the exception message,
        /// since the message could be shown on a level with lower security.
        /// Especially dangerous with faulty command texts (code injection),
        /// or commands with inline parameters. (information leak)
        /// (So there's another reason to use real parameters.)
        /// </remarks>
        public SqlHelperException(string method, string commandText, IParameter[] parameters, Exception innerException)
            : base(String.Format("SQL helper exception in {0}", method), innerException)
        {

            #if DEBUG
                // Save command and parameters
                m_CommandText = commandText;
                m_Parameters = parameters;

            #endif // DEBUG

        }

        #endregion
    }
}
