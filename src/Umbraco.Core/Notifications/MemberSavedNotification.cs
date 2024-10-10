// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberService when the Save method is called in the API and after data has been persisted.
/// </summary>
public sealed class MemberSavedNotification : SavedNotification<IMember>
{
    public MemberSavedNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberSavedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the saved collection of <see cref="IMember"/> objects.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MemberSavedNotification(IEnumerable<IMember> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
