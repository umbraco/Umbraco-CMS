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
    public MemberTypeChangedNotification(ContentTypeChange<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberTypeChangedNotification(IEnumerable<ContentTypeChange<IMemberType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
