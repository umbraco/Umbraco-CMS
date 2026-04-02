namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Specifies the type of a field in the Delivery API content index.
/// </summary>
public enum FieldType
{
    /// <summary>
    ///     A raw string field that is not analyzed.
    /// </summary>
    StringRaw,

    /// <summary>
    ///     A string field that is analyzed for full-text search.
    /// </summary>
    StringAnalyzed,

    /// <summary>
    ///     A string field that is optimized for sorting.
    /// </summary>
    StringSortable,

    /// <summary>
    ///     A numeric field.
    /// </summary>
    Number,

    /// <summary>
    ///     A date/time field.
    /// </summary>
    Date
}
