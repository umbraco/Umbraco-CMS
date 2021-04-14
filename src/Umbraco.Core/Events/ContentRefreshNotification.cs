using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class ContentRefreshNotification : EntityRefreshNotification<IContent>
    {
        public ContentRefreshNotification(IContent target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
