using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Provides a default implementation for
///     <see ref="IPropertyIndexValueFactory" />, returning a single field to index containing the property value.
/// </summary>
public class DefaultPropertyIndexValueFactory : IPropertyIndexValueFactory
{
    public IEnumerable<IndexValue> GetIndexValues(IProperty property, string? culture, string? segment, bool published,
        IEnumerable<string> availableCultures, IDictionary<Guid, IContentType> contentTypeDictionary)
        =>
        [
            new IndexValue
            {
                Culture = culture,
                FieldName = property.Alias,
                Values = [property.GetValue(culture, segment, published)]
            }
        ];
}
