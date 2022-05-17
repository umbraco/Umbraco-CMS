using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property index value factory.
/// </summary>
public interface IPropertyIndexValueFactory
{
    /// <summary>
    ///     Gets the index values for a property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Returns key-value pairs, where keys are indexed field names. By default, that would be the property alias,
    ///         and there would be only one pair, but some implementations (see for instance the grid one) may return more than
    ///         one pair, with different indexed field names.
    ///     </para>
    ///     <para>
    ///         And then, values are an enumerable of objects, because each indexed field can in turn have multiple
    ///         values. By default, there would be only one object: the property value. But some implementations may return
    ///         more than one value for a given field.
    ///     </para>
    /// </remarks>
    IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published);
}
