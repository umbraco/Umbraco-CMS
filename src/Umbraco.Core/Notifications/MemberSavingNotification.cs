// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberService when the Saving method is called in the API.
/// </summary>
public sealed class MemberSavingNotification : SavingNotification<IMember>
{
    public MemberSavingNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberSavingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IMember"/> objects being saved.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MemberSavingNotification(IEnumerable<IMember> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
