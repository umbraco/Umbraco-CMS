using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MediaRefreshNotification : EntityRefreshNotification<IMedia>
    {
        public MediaRefreshNotification(IMedia target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
