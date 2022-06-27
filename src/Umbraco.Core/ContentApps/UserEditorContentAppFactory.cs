using System.Collections.Generic;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.ContentApps
{
    public class UserEditorContentAppFactory : IContentAppFactory
    {
        internal const int Weight = -100;

        private ContentApp? _app;

        public ContentApp? GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            switch (source)
            {
                case IUser _:
                    return _app ??= new ContentApp()
                    {
                        Alias = "umbUser",
                        Name = "Detail",
                        Icon = Constants.Icons.User,
                        View = "views/users/views/user/details.html",
                        Weight = Weight
                    };
                default:
                    return null;
            }
        }
    }
}
