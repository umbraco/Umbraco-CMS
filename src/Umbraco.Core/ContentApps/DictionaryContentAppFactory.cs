using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.ContentApps;

internal class DictionaryContentAppFactory : IContentAppFactory
{
    private const int Weight = -100;

    private ContentApp? _dictionaryApp;

    public ContentApp? GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
    {
        switch (source)
        {
            case IDictionaryItem _:
                return _dictionaryApp ??= new ContentApp
                {
                    Alias = "dictionaryContent",
                    Name = "Content",
                    Icon = "icon-document",
                    View = "views/dictionary/views/content/content.html",
                    Weight = Weight,
                };
            default:
                return null;
        }
    }
}
