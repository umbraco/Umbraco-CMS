using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Describes the states of a document, with regard to (schedule) publishing.
/// </summary>
[Serializable]
[DataContract]
public enum ContentStatus
{
    // typical flow:
    // Unpublished (add release date)-> AwaitingRelease (release)-> Published (expire)-> Expired

    /// <summary>
    ///     The document is not trashed, and not published.
    /// </summary>
    [EnumMember]
    Unpublished,

    /// <summary>
    ///     The document is published.
    /// </summary>
    [EnumMember]
    Published,

    /// <summary>
    ///     The document is not trashed, not published, after being unpublished by a scheduled action.
    /// </summary>
    [EnumMember]
    Expired,

    /// <summary>
    ///     The document is trashed.
    /// </summary>
    [EnumMember]
    Trashed,

    /// <summary>
    ///     The document is not trashed, not published, and pending publication by a scheduled action.
    /// </summary>
    [EnumMember]
    AwaitingRelease,
}
