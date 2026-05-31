namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Attribute that represents a reference between two tables/DTOs
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class ReferencesAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencesAttribute"/> class, specifying the type that is referenced by the decorated property or field.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> that the decorated member references, typically representing a related entity or table.</param>
    public ReferencesAttribute(Type type) => Type = type;

    /// <summary>
    ///     Gets or sets the Type of the referenced DTO/table
    /// </summary>
    public Type Type { get; set; }
}
