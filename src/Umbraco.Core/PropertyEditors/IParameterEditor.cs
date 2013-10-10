using System.Collections.Generic;

namespace Umbraco.Core.PropertyEditors
{
    public interface IParameterEditor
    {
        /// <summary>
        /// The id  of the property editor
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// The name of the property editor
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Allows a parameter editor to be re-used based on the configuration specified.
        /// </summary>
        IDictionary<string, object> Configuration { get; }

        IValueEditor ValueEditor { get; }
    }
}