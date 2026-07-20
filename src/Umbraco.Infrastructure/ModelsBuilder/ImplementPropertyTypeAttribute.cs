namespace Umbraco.Cms.Infrastructure.ModelsBuilder;

/// <summary>
///     Indicates that a property implements a given property alias.
/// </summary>
/// <remarks>And therefore it should not be generated.</remarks>
[AttributeUsage(AttributeTargets.Property /*, AllowMultiple = false, Inherited = false*/)]
public class ImplementPropertyTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImplementPropertyTypeAttribute"/> class using the specified property alias.
    /// </summary>
    /// <param name="alias">The property alias associated with this attribute.</param>
    public ImplementPropertyTypeAttribute(string alias) => Alias = alias;

    /// <summary>
    /// Gets the alias that identifies the property type implemented by this attribute.
    /// </summary>
    public string Alias { get; }
}
