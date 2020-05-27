using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.ContentApps
{
    internal class ContentTypePermissionsContentAppFactory : IContentAppFactory
    {
        private const int Weight = -160;

        private ContentApp _contentTypeApp;

        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            switch (source)
            {
                case IContent _:
                    return null;
                case IMedia _:
                    return null;
                case IMember _:
                    return null;
                case IContentType _:
                    return _contentTypeApp ?? (_contentTypeApp = new ContentApp()
                    {
                        Alias = "permissions",
                        Name = "Permissions",
                        Icon = "icon-keychain",
                        View = "views/documenttypes/views/permissions/permissions.html",
                        Weight = Weight
                    });
                default:
                    throw new NotSupportedException($"Object type {source.GetType()} is not supported here.");
            }
        }
    }
}
