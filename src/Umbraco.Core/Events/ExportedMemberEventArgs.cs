using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Events
{
    public class ExportedMemberEventArgs : EventArgs
    {
        public IMember Member { get; }
        public MemberExportModel Exported { get; }

        public ExportedMemberEventArgs(IMember member, MemberExportModel exported)
        {
            Member = member;
            Exported = exported;
        }
    }
}
