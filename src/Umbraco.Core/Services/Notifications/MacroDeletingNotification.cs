using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class MacroDeletingNotification : DeletingNotification<IMacro>
    {
        public MacroDeletingNotification(IMacro target, EventMessages messages) : base(target, messages)
        {
        }

        public MacroDeletingNotification(IEnumerable<IMacro> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
