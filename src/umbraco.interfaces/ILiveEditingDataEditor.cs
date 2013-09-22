using System;
using System.Web.UI;

namespace umbraco.interfaces
{
    /// <summary>
    /// Data type editor controls can choose to implement this interface
    /// to customize their Live Editing behavior.
    /// </summary>
    [Obsolete("ILiveEditingDataEditor is obsolete and is no longer used, it will be removed from the codebase in future versions")]
    public interface ILiveEditingDataEditor
    {
        /// <summary>
        /// Gets the control used for Live Editing.
        /// </summary>
        Control LiveEditingControl { get; }
    }
}
