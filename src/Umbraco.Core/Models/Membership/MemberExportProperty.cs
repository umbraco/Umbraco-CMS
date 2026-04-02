namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents a property for member export data.
/// </summary>
public class MemberExportProperty
{
    /// <summary>
    ///     Gets or sets the unique identifier for the property.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the alias of the property.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    ///     Gets or sets the name of the property.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the value of the property.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the property was created.
    /// </summary>
    public DateTime? CreateDate { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the property was last updated.
    /// </summary>
    public DateTime? UpdateDate { get; set; }
}
