using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a MemberType is saved or deleted, after the transaction has completed.
///  This is mainly used for caching purposes, and generally not recommended. Use <see cref="MemberTypeSavedNotification"/> and <see cref="MemberTypeDeletedNotification"/> instead.
/// </summary>
public class MemberTypeChangedNotification : ContentTypeChangeNotification<IMemberType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeChangedNotification"/> class
    ///     with a single content type change.
    /// </summary>
    /// <param name="target">The content type change information for the member type.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeChangedNotification(ContentTypeChange<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeChangedNotification"/> class
    ///     with multiple content type changes.
    /// </summary>
    /// <param name="target">The content type change information for the member types.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeChangedNotification(IEnumerable<ContentTypeChange<IMemberType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
