using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MacroSavedNotification : SavedNotification<IMacro>
{
    public MacroSavedNotification(IMacro target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MacroSavedNotification(IEnumerable<IMacro> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
