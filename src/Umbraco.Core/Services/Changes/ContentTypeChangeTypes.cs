using System;

namespace Umbraco.Core.Services.Changes
{
    [Flags]
    public enum ContentTypeChangeTypes : byte
    {
        None = 0,
        RefreshMain = 1, // changed, impacts content (adding ppty or composition does NOT)
        RefreshOther = 2, // changed, other changes
        Remove = 4 // item type has been removed
    }
}
