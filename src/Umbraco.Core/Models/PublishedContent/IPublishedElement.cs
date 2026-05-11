namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents a published element.
/// </summary>
public interface IPublishedElement
{
    #region ContentType

    /// <summary>
    ///     Gets the content type.
    /// </summary>
    IPublishedContentType ContentType { get; }

    #endregion

    #region PublishedElement

    /// <summary>
    ///     Gets the unique key of the published element.
    /// </summary>
    Guid Key { get; }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the properties of the element.
    /// </summary>
    /// <remarks>
    ///     Contains one <c>IPublishedProperty</c> for each property defined for the content type, including
    ///     inherited properties. Some properties may have no value.
    /// </remarks>
    IEnumerable<IPublishedProperty> Properties { get; }

    /// <summary>
    ///     Gets a property identified by its alias.
    /// </summary>
    /// <param name="alias">The property alias.</param>
    /// <returns>The property identified by the alias.</returns>
    /// <remarks>
    ///     <para>If the content type has no property with that alias, including inherited properties, returns <c>null</c>,</para>
    ///     <para>otherwise return a property -- that may have no value (ie <c>HasValue</c> is <c>false</c>).</para>
    ///     <para>The alias is case-insensitive.</para>
    /// </remarks>
    IPublishedProperty? GetProperty(string alias);

    #endregion
}
