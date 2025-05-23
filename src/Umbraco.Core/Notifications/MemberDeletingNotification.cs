// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberService when the Delete and DeleteMembersOfType methods are called in the API.
/// </summary>
public sealed class MemberDeletingNotification : DeletingNotification<IMember>
{
    public MemberDeletingNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberDeletingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IMember"/> objects being deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MemberDeletingNotification(IEnumerable<IMember> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
