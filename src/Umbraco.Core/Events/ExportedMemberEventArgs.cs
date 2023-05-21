using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Events;

public class ExportedMemberEventArgs : EventArgs
{
    public ExportedMemberEventArgs(IMember member, MemberExportModel exported)
    {
        Member = member;
        Exported = exported;
    }

    public IMember Member { get; }

    public MemberExportModel Exported { get; }
}
