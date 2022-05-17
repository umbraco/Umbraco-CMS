using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.ContentApps;

public class ContentEditorContentAppFactory : IContentAppFactory
{
    // see note on ContentApp
    internal const int Weight = -100;

    private ContentApp? _contentApp;
    private ContentApp? _mediaApp;
    private ContentApp? _memberApp;

    public ContentApp? GetContentAppFor(object o, IEnumerable<IReadOnlyUserGroup> userGroups)
    {
        switch (o)
        {
            case IContent content when content.Properties.Count > 0:
                return _contentApp ??= new ContentApp
                {
                    Alias = "umbContent",
                    Name = "Content",
                    Icon = Constants.Icons.Content,
                    View = "views/content/apps/content/content.html",
                    Weight = Weight,
                };

            case IMedia media when !media.ContentType.IsContainer || media.Properties.Count > 0:
                return _mediaApp ??= new ContentApp
                {
                    Alias = "umbContent",
                    Name = "Content",
                    Icon = Constants.Icons.Content,
                    View = "views/media/apps/content/content.html",
                    Weight = Weight,
                };

            case IMember _:
                return _memberApp ??= new ContentApp
                {
                    Alias = "umbContent",
                    Name = "Content",
                    Icon = Constants.Icons.Content,
                    View = "views/member/apps/content/content.html",
                    Weight = Weight,
                };

            default:
                return null;
        }
    }
}
