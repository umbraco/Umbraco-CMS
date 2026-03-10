namespace Umbraco.Cms.Core;

/// <summary>
///     Attribute used to define a UDI (Umbraco Document Identifier) type for an entity.
/// </summary>
/// <remarks>
///     This attribute can be applied multiple times to a class to define multiple UDI types.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class UdiDefinitionAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UdiDefinitionAttribute" /> class.
    /// </summary>
    /// <param name="entityType">The entity type for the UDI.</param>
    /// <param name="udiType">The type of UDI (GUID or String).</param>
    /// <exception cref="ArgumentNullException">The entity type is null or whitespace.</exception>
    /// <exception cref="ArgumentException">The UDI type is not valid.</exception>
    public UdiDefinitionAttribute(string entityType, UdiType udiType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new ArgumentNullException("entityType");
        }

        if (udiType != UdiType.GuidUdi && udiType != UdiType.StringUdi)
        {
            throw new ArgumentException("Invalid value.", "udiType");
        }

        EntityType = entityType;
        UdiType = udiType;
    }

    /// <summary>
    ///     Gets the entity type for this UDI definition.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    ///     Gets the type of UDI (GUID or String) for this definition.
    /// </summary>
    public UdiType UdiType { get; }
}
