using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.ContentApps;

public class ContentTypeTemplatesContentAppFactory : IContentAppFactory
{
    private const int Weight = -140;

    private ContentApp? _contentTypeApp;

    public ContentApp? GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
    {
        switch (source)
        {
            case IContentType _:
                return _contentTypeApp ??= new ContentApp
                {
                    Alias = "templates",
                    Name = "Templates",
                    Icon = "icon-layout",
                    View = "views/documentTypes/views/templates/templates.html",
                    Weight = Weight,
                };
            default:
                return null;
        }
    }
}
