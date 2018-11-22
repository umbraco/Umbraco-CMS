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
    /// Interface for an <see cref="IParameter"/> that wraps around a <see cref="System.Data.IDataParameter"/>.
    /// </summary>
    /// <typeparam name="P">Data type of the wrapped parameter.</typeparam>
    public interface IParameterContainer<P> : IParameter where P : IDataParameter
    {
        /// <summary>
        /// Gets the original parameter.
        /// </summary>
        /// <value>The original parameter.</value>
        P RawParameter { get; }
    }
}
