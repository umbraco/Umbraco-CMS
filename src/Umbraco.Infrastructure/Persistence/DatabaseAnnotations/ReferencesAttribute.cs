namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Attribute that represents a reference between two tables/DTOs
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class ReferencesAttribute : Attribute
{
    public ReferencesAttribute(Type type) => Type = type;

    /// <summary>
    ///     Gets or sets the Type of the referenced DTO/table
    /// </summary>
    public Type Type { get; set; }
}
