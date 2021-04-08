using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class MacroSavingNotification : SavingNotification<IMacro>
    {
        public MacroSavingNotification(IMacro target, EventMessages messages) : base(target, messages)
        {
        }

        public MacroSavingNotification(IEnumerable<IMacro> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
