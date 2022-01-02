using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Notifications
{
    public class SendingAllowedChildrenNotification : INotification
    {
        public IUmbracoContext UmbracoContext { get; }

        public ContentTypeBasic[] Children { get; }

        public SendingAllowedChildrenNotification(ContentTypeBasic[] children, IUmbracoContext umbracoContext)
        {
            UmbracoContext = umbracoContext;
            Children = children;
        }
    }
}
