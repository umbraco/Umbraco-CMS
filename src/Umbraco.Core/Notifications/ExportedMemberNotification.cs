// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when a member has been exported.
/// </summary>
/// <remarks>
///     This notification is published after member data has been exported, typically for GDPR
///     compliance purposes. Handlers can use this to add additional data to the export.
/// </remarks>
public class ExportedMemberNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExportedMemberNotification"/> class.
    /// </summary>
    /// <param name="member">The member that was exported.</param>
    /// <param name="exported">The export model containing the member's data.</param>
    public ExportedMemberNotification(IMember member, MemberExportModel exported)
    {
        Member = member;
        Exported = exported;
    }

    /// <summary>
    ///     Gets the member that was exported.
    /// </summary>
    public IMember Member { get; }

    /// <summary>
    ///     Gets the export model containing the member's data.
    /// </summary>
    public MemberExportModel Exported { get; }
}
