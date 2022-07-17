namespace Umbraco.Cms.Infrastructure.ModelsBuilder;

/// <summary>
///     Indicates that a property implements a given property alias.
/// </summary>
/// <remarks>And therefore it should not be generated.</remarks>
[AttributeUsage(AttributeTargets.Property /*, AllowMultiple = false, Inherited = false*/)]
public class ImplementPropertyTypeAttribute : Attribute
{
    public ImplementPropertyTypeAttribute(string alias) => Alias = alias;

    public string Alias { get; }
}
