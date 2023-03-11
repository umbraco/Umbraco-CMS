using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Notifications;

public class SendingSectionsNotification : INotification
{
    public SendingSectionsNotification(IEnumerable<Section> sections, IUmbracoContext umbracoContext)
    {
        Sections = sections;
        UmbracoContext = umbracoContext;
    }

    public IUmbracoContext UmbracoContext { get; }

    public IEnumerable<Section> Sections { get; set; }
}
