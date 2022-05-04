namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Attribute that represents a db constraint
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ConstraintAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the name of the constraint
    /// </summary>
    /// <remarks>
    ///     Overrides the default naming of a property constraint:
    ///     DF_tableName_propertyName
    /// </remarks>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the Default value
    /// </summary>
    public object? Default { get; set; }
}
