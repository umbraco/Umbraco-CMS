using System.Collections.Generic;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.ContentApps
{
    public class UserGroupEditorContentAppFactory : IContentAppFactory
    {
        internal const int Weight = -100;

        private ContentApp? _app;

        public ContentApp? GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            switch (source)
            {
                case IUserGroup _:
                    return _app ??= new ContentApp()
                    {
                        Alias = "umbUserGroup",
                        Name = "Detail",
                        Icon = Constants.Icons.UserGroup,
                        View = "views/users/apps/groups/content/content.html",
                        Weight = Weight
                    };
                default:
                    return null;
            }
        }
    }
}
