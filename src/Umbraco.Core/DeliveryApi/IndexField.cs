namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Represents a field definition for the Delivery API content index.
/// </summary>
public sealed class IndexField
{
    /// <summary>
    ///     Gets or sets the name of the index field.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    ///     Gets or sets the type of the index field.
    /// </summary>
    public required FieldType FieldType { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this field varies by culture.
    /// </summary>
    public required bool VariesByCulture { get; set; }
}
