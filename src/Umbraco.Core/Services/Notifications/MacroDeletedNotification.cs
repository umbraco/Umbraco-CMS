using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class MacroDeletedNotification : DeletedNotification<IMacro>
    {
        public MacroDeletedNotification(IMacro target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
