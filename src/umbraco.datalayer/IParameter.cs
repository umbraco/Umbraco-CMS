/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Data;

namespace umbraco.DataLayer
{
    /// <summary>
    /// Interface for a parameter of a data layer instruction.
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <value>The name of the parameter.</value>
        string ParameterName { get; }

        /// <summary>
        /// Gets the value of the parameter.
        /// </summary>
        /// <value>The value of the parameter.</value>
        object Value { get; }
    }
}
