using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.ContentApps;

internal class MemberEditorContentAppFactory : IContentAppFactory
{
    // see note on ContentApp
    internal const int Weight = +50;

    private ContentApp? _memberApp;

    public ContentApp? GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
    {
        switch (source)
        {
            case IMember _:
                return _memberApp ??= new ContentApp
                {
                    Alias = "umbMembership",
                    Name = "Member",
                    Icon = "icon-user",
                    View = "views/member/apps/membership/membership.html",
                    Weight = Weight,
                };

            default:
                return null;
        }
    }
}
