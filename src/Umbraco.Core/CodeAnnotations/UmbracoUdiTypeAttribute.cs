namespace Umbraco.Cms.Core.CodeAnnotations;

/// <summary>
/// Attribute to associate a UDI (Umbraco Document Identifier) type string with an enum value.
/// </summary>
/// <remarks>
/// UDI types are string identifiers used in the Umbraco unified identifier system.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public class UmbracoUdiTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoUdiTypeAttribute"/> class.
    /// </summary>
    /// <param name="udiType">The UDI type string.</param>
    public UmbracoUdiTypeAttribute(string udiType) => UdiType = udiType;

    /// <summary>
    /// Gets the UDI type string.
    /// </summary>
    public string UdiType { get; }
}
