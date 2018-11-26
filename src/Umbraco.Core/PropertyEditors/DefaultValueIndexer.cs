using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Returns a single field to index containing the property value
    /// </summary>
    public class DefaultValueIndexer : IValueIndexer
    {
        public IEnumerable<KeyValuePair<string, object[]>> GetIndexValues(Property property, string culture, string segment)
        {
            yield return new KeyValuePair<string, object[]>(property.Alias, new[] { property.GetValue(culture, segment) });
        }
    }
}
