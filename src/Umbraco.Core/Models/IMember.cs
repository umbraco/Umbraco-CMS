using System.ComponentModel;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

public interface IMember : IContentBase, IMembershipUser, IHaveAdditionalData
{
    /// <summary>
    ///     String alias of the default ContentType
    /// </summary>
    string ContentTypeAlias { get; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string? LongStringPropertyValue { get; set; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string? ShortStringPropertyValue { get; set; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    int IntegerPropertyValue { get; set; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    bool BoolPropertyValue { get; set; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    DateTime DateTimePropertyValue { get; set; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string? PropertyTypeAlias { get; set; }
}
