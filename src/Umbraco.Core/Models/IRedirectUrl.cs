using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a redirect URL.
/// </summary>
public interface IRedirectUrl : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the identifier of the content item.
    /// </summary>
    [DataMember]
    int ContentId { get; set; }

    /// <summary>
    ///     Gets or sets the unique key identifying the content item.
    /// </summary>
    [DataMember]
    Guid ContentKey { get; set; }

    /// <summary>
    ///     Gets or sets the redirect URL creation date.
    /// </summary>
    [DataMember]
    DateTime CreateDateUtc { get; set; }

    /// <summary>
    ///     Gets or sets the culture.
    /// </summary>
    [DataMember]
    string? Culture { get; set; }

    /// <summary>
    ///     Gets or sets the redirect URL route.
    /// </summary>
    /// <remarks>Is a proper Umbraco route eg /path/to/foo or 123/path/tofoo.</remarks>
    [DataMember]
    string Url { get; set; }
}
