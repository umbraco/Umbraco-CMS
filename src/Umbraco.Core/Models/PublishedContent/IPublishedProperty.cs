namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents a property of an <c>IPublishedElement</c>.
/// </summary>
public interface IPublishedProperty
{
    IPublishedPropertyType PropertyType { get; }

    /// <summary>
    ///     Gets the alias of the property.
    /// </summary>
    string Alias { get; }

    /// <summary>
    ///     Gets a value indicating whether the property has a value.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is somewhat implementation-dependent -- depending on whatever IPublishedCache considers
    ///         a missing value.
    ///     </para>
    ///     <para>
    ///         The XmlPublishedCache raw values are strings, and it will consider missing, null or empty (and
    ///         that includes whitespace-only) strings as "no value".
    ///     </para>
    ///     <para>
    ///         Other caches that get their raw value from the database would consider that a property has "no
    ///         value" if it is missing, null, or an empty string (including whitespace-only).
    ///     </para>
    /// </remarks>
    bool HasValue(string? culture = null, string? segment = null);

    /// <summary>
    ///     Gets the source value of the property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The source value is whatever was passed to the property when it was instantiated, and it is
    ///         somewhat implementation-dependent -- depending on how the IPublishedCache is implemented.
    ///     </para>
    ///     <para>The XmlPublishedCache source values are strings exclusively since they come from the Xml cache.</para>
    ///     <para>
    ///         For other caches that get their source value from the database, it would be either a string,
    ///         an integer (Int32), a date and time (DateTime) or a decimal (double).
    ///     </para>
    ///     <para>
    ///         If you're using that value, you're probably wrong, unless you're doing some internal
    ///         Umbraco stuff.
    ///     </para>
    /// </remarks>
    object? GetSourceValue(string? culture = null, string? segment = null);

    /// <summary>
    ///     Gets the object value of the property.
    /// </summary>
    /// <remarks>
    ///     <para>The value is what you want to use when rendering content in an MVC view ie in C#.</para>
    ///     <para>It can be null, or any type of CLR object.</para>
    ///     <para>It has been fully prepared and processed by the appropriate converter.</para>
    /// </remarks>
    object? GetValue(string? culture = null, string? segment = null);

    /// <summary>
    ///     Gets the XPath value of the property.
    /// </summary>
    /// <remarks>
    ///     <para>The XPath value is what you want to use when navigating content via XPath eg in the XSLT engine.</para>
    ///     <para>It must be either null, or a string, or an XPathNavigator.</para>
    ///     <para>It has been fully prepared and processed by the appropriate converter.</para>
    /// </remarks>
    object? GetXPathValue(string? culture = null, string? segment = null);
}
