using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Notifications;

public class SendingAllowedChildrenNotification : INotification
{
    public SendingAllowedChildrenNotification(IEnumerable<ContentTypeBasic> children, IUmbracoContext umbracoContext)
    {
        UmbracoContext = umbracoContext;
        Children = children;
    }

    public IUmbracoContext UmbracoContext { get; }

    public IEnumerable<ContentTypeBasic> Children { get; set; }
}
