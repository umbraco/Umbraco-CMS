using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Returns indexable data for the property
    /// </summary>
    public interface IValueIndexer
    {
        //fixme: What about segments and whether we want the published value?
        IEnumerable<KeyValuePair<string, object[]>> GetIndexValues(Property property, string culture, string segment);
    }
}
