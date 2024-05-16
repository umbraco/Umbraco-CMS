// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberService when the Delete and DeleteMembersOfType methods are called in the API, after the members have been deleted.
/// </summary>
public sealed class MemberDeletedNotification : DeletedNotification<IMember>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberDeletedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of deleted <see cref="IMember"/> objects.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MemberDeletedNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }
}
