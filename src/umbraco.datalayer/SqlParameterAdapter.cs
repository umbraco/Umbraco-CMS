/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System.Data;

namespace umbraco.DataLayer
{
    /// <summary>
    /// Generic class adapter to ISqlParameterContainer for parameters implementing IDataParameter.
    /// </summary>
    /// <typeparam name="P">SQL parameter data type</typeparam>
    public class SqlParameterAdapter<P> : IParameterContainer<P> where P : IDataParameter
    {
        #region Private Fields

        /// <summary>The original parameter.</summary>
        private readonly P m_RawParameter; 

        #endregion

        #region Public Properties
       
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <value>The name of the parameter.</value>
        public string ParameterName
        {
            get { return m_RawParameter.ParameterName; }
        }

        /// <summary>
        /// Gets the value of the parameter.
        /// </summary>
        /// <value>The value of the parameter.</value>
        public object Value
        {
            get { return m_RawParameter.Value; }
        }

        /// <summary>
        /// Gets the wrapped parameter.
        /// </summary>
        /// <value>The wrapped parameter.</value>
        public P RawParameter
        {
            get { return m_RawParameter; }
        } 

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlParameterAdapter&lt;P&gt;"/> class.
        /// </summary>
        /// <param name="rawParameter">The raw parameter.</param>
        public SqlParameterAdapter(P rawParameter)
        {
            m_RawParameter = rawParameter;
        }

        #endregion
    }
}
