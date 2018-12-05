using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a property value indexer.
    /// </summary>
    public interface IPropertyIndexValueFactory
    {
        /// <summary>
        /// Gets the index values for a property.
        /// </summary>
        IEnumerable<KeyValuePair<string, IEnumerable<object>>> GetIndexValues(Property property, string culture, string segment, bool published);
    }
}
