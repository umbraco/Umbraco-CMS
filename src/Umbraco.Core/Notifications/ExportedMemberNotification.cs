using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

public class ExportedMemberNotification : INotification
{
    public ExportedMemberNotification(IMember member, MemberExportModel exported)
    {
        Member = member;
        Exported = exported;
    }

    public IMember Member { get; }

    public MemberExportModel Exported { get; }
}
