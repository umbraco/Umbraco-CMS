using System;

namespace Umbraco.Core.Services.Changes
{
    [Flags]
    public enum ContentTypeChangeTypes : byte
    {
        None = 0,
        Create = 1, // item type has been created, no impact
        RefreshMain = 2, // changed, impacts content (adding property or composition does NOT)
        RefreshOther = 4, // changed, other changes
        Remove = 8 // item type has been removed
    }
}
