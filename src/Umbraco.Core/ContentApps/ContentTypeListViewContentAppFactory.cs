using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.ContentApps;

public class ContentTypeListViewContentAppFactory : IContentAppFactory
{
    private const int Weight = -180;

    private ContentApp? _contentTypeApp;

    public ContentApp? GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
    {
        switch (source)
        {
            case IContentType _:
                return _contentTypeApp ??= new ContentApp
                {
                    Alias = "listView",
                    Name = "List view",
                    Icon = "icon-list",
                    View = "views/documentTypes/views/listview/listview.html",
                    Weight = Weight,
                };
            default:
                return null;
        }
    }
}
