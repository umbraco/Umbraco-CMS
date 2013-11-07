namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a property of an <c>IPublishedContent</c>.
    /// </summary>
	public interface IPublishedProperty
	{
        /// <summary>
        /// Gets the alias of the property.
        /// </summary>
		string PropertyTypeAlias { get; }

        /// <summary>
        /// Gets a value indicating whether the property has a value.
        /// </summary>
        /// <remarks>
        /// <para>This is somewhat implementation-dependent -- depending on whatever IPublishedCache considers
        /// a missing value.</para>
        /// <para>The XmlPublishedCache raw values are strings, and it will consider missing, null or empty (and
        /// that includes whitespace-only) strings as "no value".</para>
        /// <para>Other caches that get their raw value from the database would consider that a property has "no
        /// value" if it is missing, null, or an empty string (including whitespace-only).</para>
        /// </remarks>
        bool HasValue { get; }

        /// <summary>
        /// Gets the data value of the property.
        /// </summary>
        /// <remarks>
        /// <para>The data value is whatever was passed to the property when it was instanciated, and it is
        /// somewhat implementation-dependent -- depending on how the IPublishedCache is implemented.</para>
        /// <para>The XmlPublishedCache raw values are strings exclusively since they come from the Xml cache.</para>
        /// <para>For other caches that get their raw value from the database, it would be either a string,
        /// an integer (Int32), or a date and time (DateTime).</para>
        /// <para>If you're using that value, you're probably wrong, unless you're doing some internal
        /// Umbraco stuff.</para>
        /// </remarks>
        object DataValue { get; }

        /// <summary>
        /// Gets the object value of the property.
        /// </summary>
        /// <remarks>
        /// <para>The value is what you want to use when rendering content in an MVC view ie in C#.</para>
        /// <para>It can be null, or any type of CLR object.</para>
        /// <para>It has been fully prepared and processed by the appropriate converter.</para>
        /// </remarks>
        object Value { get; }

        /// <summary>
        /// Gets the XPath value of the property.
        /// </summary>
        /// <remarks>
        /// <para>The XPath value is what you want to use when navigating content via XPath eg in the XSLT engine.</para>
        /// <para>It must be either null, or a string, or an XPathNavigator.</para>
        /// <para>It has been fully prepared and processed by the appropriate converter.</para>
        /// </remarks>
        object XPathValue { get; }
	}
}