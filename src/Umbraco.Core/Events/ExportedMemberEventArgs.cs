using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for member export operations.
/// </summary>
public class ExportedMemberEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExportedMemberEventArgs" /> class.
    /// </summary>
    /// <param name="member">The member being exported.</param>
    /// <param name="exported">The exported member data model.</param>
    public ExportedMemberEventArgs(IMember member, MemberExportModel exported)
    {
        Member = member;
        Exported = exported;
    }

    /// <summary>
    ///     Gets the member being exported.
    /// </summary>
    public IMember Member { get; }

    /// <summary>
    ///     Gets the exported member data model.
    /// </summary>
    public MemberExportModel Exported { get; }
}
