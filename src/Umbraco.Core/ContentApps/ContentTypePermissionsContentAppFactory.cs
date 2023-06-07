using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.ContentApps;

public class ContentTypePermissionsContentAppFactory : IContentAppFactory
{
    private const int Weight = -160;

    private ContentApp? _contentTypeApp;

    public ContentApp? GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
    {
        switch (source)
        {
            case IContentType _:
                return _contentTypeApp ??= new ContentApp
                {
                    Alias = "permissions",
                    Name = "Permissions",
                    Icon = "icon-keychain",
                    View = "views/documentTypes/views/permissions/permissions.html",
                    Weight = Weight,
                };
            default:
                return null;
        }
    }
}
