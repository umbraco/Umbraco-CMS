using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.ContentApps
{
    internal class MemberEditorContentAppFactory : IContentAppFactory
    {
        // see note on ContentApp
        internal const int Weight = +50;

        private ContentApp _memberApp;

        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            switch (source)
            {
                case IMember _:
                    return _memberApp ?? (_memberApp = new ContentApp
                    {
                        Alias = "umbMembership",
                        Name = "Member",
                        Icon = "icon-user",
                        View = "views/member/apps/membership/membership.html",
                        Weight = Weight
                    });

                default:
                    return null;
            }
        }
    }
}
