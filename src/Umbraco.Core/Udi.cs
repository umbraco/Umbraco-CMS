using System.ComponentModel;

namespace Umbraco.Cms.Core;

/// <summary>
///     Represents an entity identifier.
/// </summary>
/// <remarks>An Udi can be fully qualified or "closed" eg umb://document/{guid} or "open" eg umb://document.</remarks>
[TypeConverter(typeof(UdiTypeConverter))]
public abstract class Udi : IComparable<Udi>
{
    /// <summary>
    ///     Initializes a new instance of the Udi class.
    /// </summary>
    /// <param name="entityType">The entity type part of the identifier.</param>
    /// <param name="stringValue">The string value of the identifier.</param>
    protected Udi(string entityType, string stringValue)
    {
        EntityType = entityType;
        UriValue = new Uri(stringValue);
    }

    /// <summary>
    ///     Initializes a new instance of the Udi class.
    /// </summary>
    /// <param name="uriValue">The uri value of the identifier.</param>
    protected Udi(Uri uriValue)
    {
        EntityType = uriValue.Host;
        UriValue = uriValue;
    }

    public Uri UriValue { get; }

    /// <summary>
    ///     Gets the entity type part of the identifier.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    ///     Gets a value indicating whether this Udi is a root Udi.
    /// </summary>
    /// <remarks>A root Udi points to the "root of all things" for a given entity type, e.g. the content tree root.</remarks>
    public abstract bool IsRoot { get; }

    public static bool operator ==(Udi? udi1, Udi? udi2)
    {
        if (ReferenceEquals(udi1, udi2))
        {
            return true;
        }

        if (udi1 is null || udi2 is null)
        {
            return false;
        }

        return udi1.Equals(udi2);
    }

    /// <summary>
    ///     Creates a root Udi for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The root Udi for the entity type.</returns>
    public static Udi Create(string entityType) => UdiParser.GetRootUdi(entityType);

    public int CompareTo(Udi? other) => string.Compare(UriValue.ToString(), other?.UriValue.ToString(), StringComparison.OrdinalIgnoreCase);

    public override string ToString() =>

        // UriValue is created in the ctor and is never null
        // use AbsoluteUri here and not ToString else it's not encoded!
        UriValue.AbsoluteUri;

    /// <summary>
    ///     Creates a string Udi.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="id">The identifier.</param>
    /// <returns>The string Udi for the entity type and identifier.</returns>
    public static Udi Create(string entityType, string id)
    {
        if (UdiParser.UdiTypes.TryGetValue(entityType, out UdiType udiType) == false)
        {
            throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", entityType), "entityType");
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "id");
        }

        if (udiType != UdiType.StringUdi)
        {
            throw new InvalidOperationException(string.Format(
                "Entity type \"{0}\" does not have string udis.",
                entityType));
        }

        return new StringUdi(entityType, id);
    }

    /// <summary>
    ///     Creates a Guid Udi.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="id">The identifier.</param>
    /// <returns>The Guid Udi for the entity type and identifier.</returns>
    public static Udi Create(string? entityType, Guid id)
    {
        if (entityType is null || UdiParser.UdiTypes.TryGetValue(entityType, out UdiType udiType) == false)
        {
            throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", entityType), "entityType");
        }

        if (udiType != UdiType.GuidUdi)
        {
            throw new InvalidOperationException(string.Format(
                "Entity type \"{0}\" does not have guid udis.",
                entityType));
        }

        if (id == default)
        {
            throw new ArgumentException("Cannot be an empty guid.", "id");
        }

        return new GuidUdi(entityType, id);
    }

    public static Udi Create(Uri uri)
    {
        // if it's a know type go fast and use ctors
        // else fallback to parsing the string (and guess the type)
        if (UdiParser.UdiTypes.TryGetValue(uri.Host, out UdiType udiType) == false)
        {
            throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", uri.Host), "uri");
        }

        if (udiType == UdiType.GuidUdi)
        {
            return new GuidUdi(uri);
        }

        if (udiType == UdiType.StringUdi)
        {
            return new StringUdi(uri);
        }

        throw new ArgumentException(string.Format("Uri \"{0}\" is not a valid udi.", uri));
    }

    public void EnsureType(params string[] validTypes)
    {
        if (validTypes.Contains(EntityType) == false)
        {
            throw new Exception(string.Format("Unexpected entity type \"{0}\".", EntityType));
        }
    }

    /// <summary>
    ///     Ensures that this Udi is not a root Udi.
    /// </summary>
    /// <returns>This Udi.</returns>
    /// <exception cref="Exception">When this Udi is a Root Udi.</exception>
    public Udi EnsureNotRoot()
    {
        if (IsRoot)
        {
            throw new Exception("Root Udi.");
        }

        return this;
    }

    public override bool Equals(object? obj)
    {
        var other = obj as Udi;
        return other is not null && GetType() == other.GetType() && UriValue == other.UriValue;
    }

    public override int GetHashCode() => UriValue.GetHashCode();

    public static bool operator !=(Udi? udi1, Udi? udi2) => udi1 == udi2 == false;
}
