using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public interface IParameterEditor : IDiscoverable
    {
        /// <summary>
        /// Gets the unique identifier of the editor.
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Gets the name of the editor.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Allows a parameter editor to be re-used based on the configuration specified. FIXME WTF?!
        /// </summary>
        IDictionary<string, object> Configuration { get; }

        IValueEditor ValueEditor { get; }
    }
}
