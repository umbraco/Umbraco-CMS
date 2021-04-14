using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MemberRefreshNotification : EntityRefreshNotification<IMember>
    {
        public MemberRefreshNotification(IMember target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
