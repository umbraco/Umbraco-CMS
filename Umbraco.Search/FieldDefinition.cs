using Umbraco.Search.Enums;

namespace Umbraco.Search;

public class FieldDefinition : IEquatable<FieldDefinition>
{
    public FieldDefinition(string name, UmbracoFieldType? type)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        Name = name;
        Type = type ?? UmbracoFieldType.Raw;
    }

    /// <summary>
    /// The name of the index field
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The data type
    /// </summary>
    public UmbracoFieldType Type { get; }

    public bool Equals(FieldDefinition? other) => string.Equals(Name, other?.Name) && string.Equals(Type, other?.Type);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is FieldDefinition definition && Equals(definition);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Name.GetHashCode() * 397) ^ Type.GetHashCode();
        }
    }

    public static bool operator ==(FieldDefinition left, FieldDefinition right) => left.Equals(right);

    public static bool operator !=(FieldDefinition left, FieldDefinition right) => !left.Equals(right);
}
