using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Returns a single field to index containing the property value
    /// </summary>
    public class DefaultPropertyIndexValues : IPropertyIndexValues
    {
        public IEnumerable<KeyValuePair<string, IEnumerable<object>>> GetIndexValues(Property property, string culture, string segment, bool published)
        {
            yield return new KeyValuePair<string, IEnumerable<object>>(
                property.Alias,
                property.GetValue(culture, segment, published).Yield());
        }
    }
}
