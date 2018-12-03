using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Returns indexable data for the property
    /// </summary>
    public interface IPropertyIndexValues
    {
        IEnumerable<KeyValuePair<string, object[]>> GetIndexValues(Property property, string culture, string segment);
    }
}
